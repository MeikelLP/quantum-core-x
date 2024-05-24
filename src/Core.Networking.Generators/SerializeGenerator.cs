using System.Text;

namespace QuantumCore.Networking;

internal class SerializeGenerator
{
    public string Generate(PacketTypeInfo typeInfo, PacketInfo packetInfo)
    {
        var dynamicByteIndex = new StringBuilder();
        var source = new StringBuilder();
        ApplyHeader(source, typeInfo);
        source.AppendLine($$"""
                                public static implicit operator byte[] ({{typeInfo.Name}} packet)
                                {
                                    var bytes = ArrayPool<byte>.Shared.Rent(packet.GetSize());
                                    packet.Serialize(bytes);
                                    return bytes;
                                }
                            """);
        source.AppendLine();
        source.AppendLine("    public void Serialize(Span<byte> bytes)");
        source.AppendLine("    {");
        source.AppendLine(GenerateWriteHeader(packetInfo.Header, packetInfo.SubHeader));
        var staticByteIndex = packetInfo.SubHeader is not null ? 2 : 1;
        foreach (var field in typeInfo.Fields)
        {
            var fieldExpression = typeInfo.Fields.Any(x => x.SizeFieldName == field.FieldName)
                ? "this.GetSize()"
                : $"this.{field.FieldName}";

            // TODO subheader
            // if (packetInfo.SubHeader is not null && packetInfo.SubHeader.Position == index)
            // {
            //     source.AppendLine(
            //         $"            bytes[offset + {staticByteIndex}] = 0x{Convert.ToString(subHeader.Value.Value, 16).PadLeft(2, '0')};");
            //     staticByteIndex++;
            // }

            var line = GenerateMethodLine(field, fieldExpression, ref staticByteIndex, dynamicByteIndex, "",
                "        ");
            source.AppendLine(line);
        }

        if (packetInfo.HasSequence)
        {
            source.AppendLine($"        bytes[{staticByteIndex}{dynamicByteIndex}] = default;");
        }

        source.AppendLine("    }");
        source.AppendLine();
        GenerateGetSizeMethod(typeInfo, source, dynamicByteIndex.ToString(), packetInfo.SubHeader is not null,
            packetInfo.HasSequence);
        ApplyFooter(source);

        return source.ToString();
    }

    internal string GenerateMethodLine(FieldData field, string fieldExpression, ref int offset,
        StringBuilder dynamicOffset, string tempDynamicOffset, string indentPrefix)
    {
        var finalLine = field switch
        {
            // handle Custom[]
            {IsArray: true} => GetLineForArray(field, fieldExpression, ref offset, dynamicOffset, tempDynamicOffset,
                indentPrefix),
            // handle string
            {TypeFullName: "System.String"} => GetLineForString(field, fieldExpression, ref offset, dynamicOffset,
                tempDynamicOffset, indentPrefix),
            // handle enum
            {IsEnum: true} => GetLineForSingleValue(field, fieldExpression,
                ref offset, dynamicOffset, tempDynamicOffset, indentPrefix),
            // misc
            _ => GenerateLineForMisc(field, fieldExpression, ref offset, dynamicOffset, tempDynamicOffset, indentPrefix)
        };

        if (!field.IsArray)
        {
            offset += field.ElementSize;
        }

        // handle anything else
        return finalLine;
    }

    internal static string GetLineForString(FieldData fieldData, string fieldExpression, ref int offset,
        StringBuilder dynamicOffset,
        string tempDynamicOffset, string indentPrefix)
    {
        var offsetStr = $"{offset}{dynamicOffset}{tempDynamicOffset}";
        if (fieldData.HasDynamicLength)
        {
            // only append to dynamic offset if type has non static length
            dynamicOffset.Append($" + {fieldExpression}.Length");
        }

        var lengthString = fieldData.HasDynamicLength
            ? $"(int)this.{fieldData.SizeFieldName} + 1"
            : fieldData.ElementSize.ToString();
        return $"{indentPrefix}bytes[{offsetStr}..({offsetStr} + {lengthString})].WriteString({fieldExpression});";
    }

    internal static string GetLineForFixedByteArray(FieldData field, string fieldExpression,
        ref int offset,
        StringBuilder dynamicOffset, string tempDynamicOffset, string indentPrefix)
    {
        var offsetStr = $"{offset}{dynamicOffset}{tempDynamicOffset}";
        offset += field.ArrayLength!.Value;
        return $"{indentPrefix}{fieldExpression}.CopyTo(bytes[{offsetStr}..]);";
    }

    internal static string GetLineForDynamicByteArray(string fieldExpression,
        ref int offset,
        StringBuilder dynamicOffset, string tempDynamicOffset, string indentPrefix)
    {
        var offsetStr = $"{offset}{dynamicOffset}{tempDynamicOffset}";
        dynamicOffset.Append($" + {fieldExpression}.Length");
        return $"{indentPrefix}{fieldExpression}.CopyTo(bytes[{offsetStr}..)]);";
    }

    internal static string GetLineForSingleValue(FieldData fieldData,
        string fieldExpression,
        ref int offset,
        StringBuilder dynamicOffset, string tempDynamicOffset, string indentPrefix)
    {
        var offsetStr = $"{offset}{dynamicOffset}{tempDynamicOffset}";
        var type = fieldData.IsEnum
            ? fieldData.ElementTypeFullName!
            : fieldData.TypeFullName;
        var cast = fieldData.IsEnum
            ? $"({fieldData.ElementTypeFullName})"
            : "";

        if (GeneratorConstants.SupportedTypesByBitConverter.Contains(type))
        {
            if (type is "System.Int32" or "System.UInt32" or
                "System.Int16" or "System.UInt16" or
                "System.Int64" or "System.UInt64")
            {
                var sb = new StringBuilder();
                var elementSize = GeneratorConstants.GetSizeOfPrimitiveType(type);
                for (var i = 0; i < elementSize; i++)
                {
                    var offsetStrLocal = $"{offset + i}{dynamicOffset}{tempDynamicOffset}";
                    var line =
                        $"{indentPrefix}bytes[{offsetStrLocal}] = (System.Byte)({cast}{fieldExpression} >> {8 * i});";

                    if (i < elementSize - 1)
                    {
                        sb.AppendLine(line);
                    }
                    else
                    {
                        sb.Append(line);
                    }
                }

                return sb.ToString();
            }
            else
            {
                return
                    $"{indentPrefix}System.BitConverter.GetBytes({cast}{fieldExpression}).CopyTo(bytes[{offsetStr}..)]);";
            }
        }

        if (GeneratorConstants.NoCastTypes.Contains(type))
        {
            offset++;
            return fieldData.IsEnum
                ? $"{indentPrefix}bytes[{offsetStr}] = {cast}{fieldExpression};"
                : $"{indentPrefix}bytes[{offsetStr}] = {fieldExpression};";
        }

        if (type == "System.String")
        {
            return GetLineForString(fieldData, fieldExpression, ref offset, dynamicOffset, tempDynamicOffset,
                indentPrefix);
        }

        if (type == "System.Boolean")
        {
            return $"{indentPrefix}bytes[{offsetStr}] = (byte)({fieldExpression} ? 1 : 0);";
        }

        throw new InvalidOperationException($"Don't know how to handle type {fieldData.TypeFullName}");
    }

    private string GenerateLineForMisc(FieldData field, string fieldExpression, ref int offset,
        StringBuilder dynamicOffset,
        string tempDynamicOffset, string indentPrefix = "")
    {
        if (field is {IsCustom: true, SubFields: not null})
        {
            // handle custom type
            var lines = new StringBuilder();
            for (var i = 0; i < field.SubFields.Length; i++)
            {
                var subField = field.SubFields[i];
                var subLine = GenerateMethodLine(subField, $"{fieldExpression}.{subField.FieldName}", ref offset,
                    dynamicOffset, tempDynamicOffset, indentPrefix);

                if (i < field.SubFields.Length - 1)
                {
                    lines.AppendLine(subLine);
                }
                else
                {
                    lines.Append(subLine);
                }
            }

            return lines.ToString();
        }

        return GetLineForSingleValue(field, fieldExpression, ref offset, dynamicOffset,
            tempDynamicOffset, indentPrefix);
    }

    private string GetLineForArray(FieldData field, string fieldExpression, ref int offset, StringBuilder dynamicOffset,
        string tempDynamicOffset, string indentPrefix)
    {
        if (field.IsArray)
        {
            if (field.ArrayLength.HasValue)
            {
                return GetLineForFixedArray(field, fieldExpression, ref offset, dynamicOffset, tempDynamicOffset,
                    indentPrefix);
            }
            else
            {
                return GetLineForDynamicArray(field, fieldExpression, ref offset, dynamicOffset, indentPrefix);
            }
        }

        throw new InvalidOperationException(
            $"Cannot get line for array if field is not array. Field is of type {field.TypeFullName}");
    }

    private string GetLineForDynamicArray(FieldData field, string fieldExpression, ref int offset,
        StringBuilder dynamicOffset, string indentPrefix)
    {
        var subTypeFullName = field.ElementTypeFullName;
        if (subTypeFullName == "System.Byte")
        {
            return GetLineForDynamicByteArray(fieldExpression, ref offset, dynamicOffset, "", indentPrefix);
        }
        else if (subTypeFullName is not null)
        {
            var lines = new StringBuilder();
            lines.AppendLine($"{indentPrefix}for (var i = 0; i < {fieldExpression}.Length; i++)");
            lines.AppendLine($"{indentPrefix}{{");

            if (GeneratorConstants.IsCustomType(subTypeFullName) && field.ElementTypeFields is not null)
            {
                // recursive call to generate lines for each field in sub type
                for (var ii = 0; ii < field.ElementTypeFields.Length; ii++)
                {
                    var member = field.ElementTypeFields[ii];
                    var subFieldExpression = $"{fieldExpression}[i].{member.FieldName}";
                    var line = GenerateMethodLine(member, subFieldExpression, ref offset, dynamicOffset,
                        $" + i * {field.ElementSize}", $"{indentPrefix}    ");
                    lines.AppendLine(line);
                }

                offset -= field.ElementSize; // reduce the offset after the array to make the offset correct
                dynamicOffset.Append($" + {fieldExpression}.Length * {field.ElementSize}");
            }
            else
            {
                var subFieldExpression = $"{fieldExpression}[i]";
                var line = GetLineForSingleValue(field, subFieldExpression, ref offset, dynamicOffset,
                    $" + i * {field.ElementSize}", $"{indentPrefix}    ");
                dynamicOffset.Append($" + {fieldExpression}.Length * {field.ElementSize}");
                lines.AppendLine(line);
            }

            lines.Append($"{indentPrefix}}}");
            return lines.ToString();
        }
        else
        {
            throw new InvalidOperationException(
                "Array must be of type byte[] or have a element type. This should never happen");
        }
    }

    private string GetLineForFixedArray(FieldData field, string fieldExpression, ref int offset,
        StringBuilder dynamicOffset, string tempDynamicOffset, string indentPrefix)
    {
        var lines = new StringBuilder();

        if (field.ElementTypeFullName == "System.Byte")
        {
            return GetLineForFixedByteArray(field, fieldExpression, ref offset, dynamicOffset, tempDynamicOffset,
                indentPrefix);
        }
        else if (field.ElementTypeFullName is not null)
        {
            // iterate over each item in array
            for (var i = 0; i < field.ArrayLength!.Value; i++)
            {
                if (GeneratorConstants.IsCustomType(field.ElementTypeFullName) && field.ElementTypeFields is not null)
                {
                    // recursive call to generate lines for each field in sub type
                    for (var ii = 0; ii < field.ElementTypeFields.Length; ii++)
                    {
                        var member = field.ElementTypeFields[ii];
                        var line = GenerateMethodLine(member, $"{fieldExpression}[{i}].{member.FieldName}", ref offset,
                            dynamicOffset,
                            tempDynamicOffset, indentPrefix);
                        if (i < field.ArrayLength!.Value - 1)
                        {
                            lines.AppendLine(line);
                        }
                        else
                        {
                            lines.AppendLine(line);
                        }
                    }
                }
                else
                {
                    var subFieldExpression = $"{fieldExpression}[{i}]";
                    var line = GetLineForSingleValue(field, subFieldExpression, ref offset, dynamicOffset,
                        tempDynamicOffset, indentPrefix);
                    offset += field.ElementSize;
                    lines.AppendLine(line);
                }
            }
        }
        else
        {
            throw new InvalidOperationException($"Given field is not of type array: {field.FieldName}");
        }

        return lines.ToString().TrimEnd();
    }

    private void GenerateGetSizeMethod(PacketTypeInfo packetTypeInfo, StringBuilder sb, string dynamicSize,
        bool hasSubHeader, bool hasSequence)
    {
        var size = packetTypeInfo.StaticSize + 1; // + header
        if (hasSubHeader)
        {
            size++;
        }

        if (hasSequence)
        {
            size++;
        }

        sb.AppendLine("    public ushort GetSize()");
        sb.AppendLine("    {");

        var dynamicString = !string.IsNullOrWhiteSpace(dynamicSize)
            ? dynamicSize
            : "";

        if (packetTypeInfo.Fields.Any(x => x.TypeFullName == "System.String" && x.HasDynamicLength))
        {
            dynamicString = $"{dynamicString} + 1";
        }

        var body = dynamicString != ""
            ? $"        return (ushort)({size}{dynamicString});"
            : $"        return {size.ToString()};";
        sb.AppendLine(body);

        sb.AppendLine("    }");
    }

    internal static string GenerateWriteHeader(byte header, byte? subHeader)
    {
        var sb = new StringBuilder();
        sb.Append($"        bytes[0] = 0x{header:X2};");
        if (subHeader is not null)
        {
            sb.AppendLine();
            sb.Append($"        bytes[1] = 0x{subHeader:X2};");
        }

        return sb.ToString();
    }

    private static void ApplyHeader(StringBuilder sb, PacketTypeInfo packetTypeInfo)
    {
        sb.AppendLine("""
                      /// <auto-generated/>
                      using System;
                      using System.Buffers;
                      using QuantumCore.Networking;

                      """);
        sb.AppendLine($"namespace {packetTypeInfo.Namespace};");
        sb.AppendLine();

        if (packetTypeInfo.StaticSize.HasValue)
        {
            sb.AppendLine($"[PacketData(StaticSize = {packetTypeInfo.StaticSize:D})]");
        }

        sb.AppendLine($"{packetTypeInfo.Modifiers} class {packetTypeInfo.Name}");
        sb.AppendLine("{");
    }

    public static void ApplyFooter(StringBuilder source)
    {
        source.AppendLine("}");
    }
}