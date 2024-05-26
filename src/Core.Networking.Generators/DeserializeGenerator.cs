using System.Text;
using System.Text.RegularExpressions;

namespace QuantumCore.Networking;

internal class DeserializeGenerator
{
    private readonly GeneratorContext _context;
    private static readonly Regex TrimAssignmentRegex = new(@"(\s+) = (.+)", RegexOptions.Compiled);

    public DeserializeGenerator(GeneratorContext context)
    {
        _context = context;
    }

    public string Generate(PacketTypeInfo packetTypeInfo)
    {
        var source = new StringBuilder();
        ApplyHeader(source, packetTypeInfo);

        source.AppendLine($"    public {packetTypeInfo.Name}()");
        source.AppendLine("    {");
        source.AppendLine("    }");
        source.AppendLine();
        source.AppendLine($"    public {packetTypeInfo.Name}(ReadOnlySpan<byte> bytes)");
        source.AppendLine("    {");
        source.AppendLine(GenerateMethodBody(packetTypeInfo, "        ", false));
        source.AppendLine("    }");
        source.AppendLine();
        source.AppendLine($"    public void Deserialize(ReadOnlySpan<byte> bytes)");
        source.AppendLine("    {");
        source.AppendLine(GenerateMethodBody(packetTypeInfo, "        ", false));
        source.AppendLine("    }");

        ApplyFooter(source);

        return source.ToString();
    }

    private string GenerateMethodBody(PacketTypeInfo packetTypeInfo, string indentPrefix,
        bool isStreamMode)
    {
        var staticByteIndex = 0;
        var dynamicByteIndex = new StringBuilder();
        var typeStaticSize = packetTypeInfo.Fields.Sum(x => x.ElementSize);
        var fieldsCopy = packetTypeInfo.Fields.ToArray();
        var sb = new StringBuilder();
        // declare and initialize variables
        foreach (var field in packetTypeInfo.Fields)
        {
            var line = GetMethodLine(field, ref staticByteIndex, dynamicByteIndex, "", indentPrefix, true,
                isStreamMode);

            // packets dynamic strings will send their size in an early field but this field is the size of the whole
            // packet not just the dynamic field's size
            var isDynamicSizeField = fieldsCopy.Any(x => x.SizeFieldName == field.FieldName);
            // + 1 because string includes a 0 byte at the end
            var staticSizeString = isDynamicSizeField ? $" - {typeStaticSize + 1}" : "";
            sb.Append($"{indentPrefix}{field.FieldName} = {line}{staticSizeString}");

            if (field is not {IsArray: true, HasDynamicLength: true} or
                {IsArray: true, ElementTypeFullName: "System.Byte"})
            {
                // dynamic arrays have a for loop after their declaration so don't put a ;
                sb.AppendLine(";");
            }
            else
            {
                sb.AppendLine();
            }
        }

        return sb.ToString().TrimEnd();
    }

    internal static string GetStreamReaderLine(FieldData field)
    {
        var typeName = field.IsArray
            ? field.ElementTypeFullName!
            : field.TypeFullName;
        if (field.IsEnum)
        {
            return $"({typeName}) await stream.ReadEnumFromStreamAsync<{typeName}>(buffer)";
        }

        if (field.IsArray && field.ElementTypeFullName == "System.Byte")
        {
            // special handling for byte arrays
            // other array values are returned as usual when in stream mode

            if (field.HasDynamicLength)
            {
                return
                    $"await stream.ReadByteArrayFromStreamAsync(buffer, {GetVariableNameForExpression(field.SizeFieldName!)})";
            }
            else
            {
                return $"await stream.ReadByteArrayFromStreamAsync(buffer, {field.ArrayLength})";
            }
        }

        if (typeName is "System.String")
        {
            var size = field.HasDynamicLength
                ? GetVariableNameForExpression(field.SizeFieldName!)
                : field.ElementSize.ToString();
            return $"await stream.ReadStringFromStreamAsync(buffer, (int){size})";
        }

        if (typeName is
            "System.Byte" or "System.SByte" or
            "System.Half" or "System.Single" or "System.Double" or
            "System.UInt16" or "System.Int16" or "System.UInt32" or "System.Int32" or "System.Int64" or "System.UInt64")
        {
            return $"await stream.ReadValueFromStreamAsync<{typeName}>(buffer)";
        }

        throw new ArgumentException($"Don't know how to handle type of field {typeName}");
    }

    internal string GetMethodLine(FieldData field, ref int offset, StringBuilder dynamicOffset,
        string tempDynamicOffset, string indentPrefix, bool isVariableMode, bool isStreamMode)
    {
        var finalLine = field switch
        {
            // handle Custom[]
            {IsArray: true} => GetLineForArray(field, ref offset, dynamicOffset, tempDynamicOffset, indentPrefix,
                isVariableMode, isStreamMode),
            // handle string
            {TypeFullName: "System.String"} => GetValueForString(field, ref offset, dynamicOffset, tempDynamicOffset,
                isStreamMode),
            // handle enum
            {IsEnum: true} => GetValueForSingleValue(field, ref offset, dynamicOffset, tempDynamicOffset, isStreamMode),
            // misc
            _ => GetLineForMisc(field, ref offset, dynamicOffset, tempDynamicOffset, indentPrefix, isVariableMode,
                isStreamMode)
        };


        if (!field.IsArray)
        {
            offset += field.ElementSize;
        }

        // handle anything else
        return finalLine;
    }

    internal static string GetValueForString(FieldData fieldData, ref int offset, StringBuilder dynamicOffset,
        string tempDynamicOffset, bool isStreamMode)
    {
        if (isStreamMode)
        {
            return GetStreamReaderLine(fieldData);
        }

        var offsetStr = GetOffsetString(offset, dynamicOffset, tempDynamicOffset);
        if (fieldData.SizeFieldName is not null)
        {
            var variableName = GetVariableNameForExpression(fieldData.SizeFieldName);
            dynamicOffset.Append($" + {variableName}");
        }

        var endOffsetStr = GetOffsetString(offset, dynamicOffset, fieldData.ElementSize > 0
            ? $"{tempDynamicOffset} + {fieldData.ElementSize}"
            : tempDynamicOffset);
        return $"bytes[{offsetStr}..{endOffsetStr}].ReadNullTerminatedString()";
    }

    internal static string GetOffsetString(in int offset, StringBuilder dynamicOffset, string tempDynamicOffset,
        int? arrayLength = null)
    {
        if (dynamicOffset.Length == 0 && tempDynamicOffset is "")
        {
            if (arrayLength is not null)
            {
                return $"{offset}..({offset} + {arrayLength})";
            }

            return offset.ToString();
        }

        if (arrayLength.HasValue)
        {
            return
                $"({offset}{dynamicOffset}{tempDynamicOffset})..({offset}{dynamicOffset}{tempDynamicOffset} + {arrayLength})";
        }
        else
        {
            return $"({offset}{dynamicOffset}{tempDynamicOffset})";
        }
    }

    internal static string GetLineForFixedByteArray(FieldData field, ref int offset, StringBuilder dynamicOffset,
        string tempDynamicOffset, bool isStreamMode)
    {
        var offsetStr = GetOffsetString(offset, dynamicOffset, tempDynamicOffset);
        var size = field.ArrayLength!.Value;
        offset += size;
        if (isStreamMode)
        {
            return $"await stream.ReadByteArrayFromStreamAsync(buffer, {size})";
        }
        else
        {
            var endOffsetStr = GetOffsetString(offset, dynamicOffset, tempDynamicOffset);
            return $"bytes[{offsetStr}..{endOffsetStr}].ToArray()";
        }
    }

    internal static string GetLineForDynamicByteArray(FieldData field, ref int offset,
        StringBuilder dynamicOffset, string tempDynamicOffset, bool isStreamMode)
    {
        var offsetStr = GetOffsetString(offset, dynamicOffset, tempDynamicOffset);
        var size = GetVariableNameForExpression(field.SizeFieldName!);
        dynamicOffset.Append($" + {size}");
        if (isStreamMode)
        {
            return $"await stream.ReadByteArrayFromStreamAsync(buffer, (int){size})";
        }
        else
        {
            var endOffsetStr = GetOffsetString(offset, dynamicOffset, tempDynamicOffset);
            return $"bytes[{offsetStr}..{endOffsetStr}].ToArray()";
        }
    }

    internal static string GetVariableNameForExpression(string fieldExpression)
    {
        return $"__{fieldExpression.Replace(".", "_")}";
    }

    internal static string GetValueForSingleValue(FieldData field, ref int offset, StringBuilder dynamicOffset,
        string tempDynamicOffset, bool isStreamMode)
    {
        if (isStreamMode)
        {
            return GetStreamReaderLine(field);
        }

        string typeName;
        if (field.IsEnum || field.IsArray)
        {
            typeName = field.ElementTypeFullName!;
        }
        else
        {
            typeName = field.TypeFullName;
        }

        int? elementSize = GeneratorConstants.SupportedTypesByBitConverter.Contains(typeName)
            ? GeneratorConstants.GetSizeOfPrimitiveType(typeName)
            : null;
        var convertExpression = typeName.Replace("System.", "");
        var offsetStr = GetOffsetString(offset, dynamicOffset, tempDynamicOffset, elementSize);
        if (field.IsEnum)
        {
            if (GeneratorConstants.SupportedTypesByBitConverter.Contains(typeName))
            {
                return $"({field.TypeFullName})System.BitConverter.To{convertExpression}(bytes[{offsetStr}])";
            }

            if (GeneratorConstants.ConvertTypes.Contains(typeName))
            {
                return $"({field.TypeFullName})System.Convert.To{convertExpression}(bytes[{offsetStr}])";
            }

            if (GeneratorConstants.NoCastTypes.Contains(typeName))
            {
                return $"({field.TypeFullName})bytes[{offsetStr}]";
            }
        }

        if (GeneratorConstants.SupportedTypesByBitConverter.Contains(typeName))
        {
            return $"System.BitConverter.To{convertExpression}(bytes[{offsetStr}])";
        }

        if (GeneratorConstants.NoCastTypes.Contains(typeName))
        {
            return $"bytes[{offsetStr}]";
        }

        if (GeneratorConstants.ConvertTypes.Contains(typeName))
        {
            return $"System.Convert.To{convertExpression}(bytes[{offsetStr}])";
        }

        if (typeName == "System.String")
        {
            return GetValueForString(field, ref offset, dynamicOffset, tempDynamicOffset, isStreamMode);
        }

        throw new InvalidOperationException($"Don't know how to handle type {typeName}");
    }

    private string GetLineForMisc(FieldData field, ref int offset, StringBuilder dynamicOffset,
        string tempDynamicOffset, string indentPrefix, bool isVariableMode, bool isStreamMode)
    {
        if (field.IsCustom)
        {
            // TODO
            return "";
        }

        return GetValueForSingleValue(field, ref offset, dynamicOffset, tempDynamicOffset, isStreamMode);
    }

    private string GetLineForArray(FieldData field, ref int offset, StringBuilder dynamicOffset,
        string tempDynamicOffset, string indentPrefix, bool isVariableMode, bool isStreamMode)
    {
        if (!field.IsArray)
        {
            throw new InvalidOperationException($"Called array method on a non array field. Field: {field.FieldName}");
        }

        if (!isVariableMode)
        {
            return GetVariableNameForExpression(field.FieldName);
        }

        if (field.ArrayLength.HasValue)
        {
            return GetLineForFixedArray(field, ref offset, dynamicOffset, tempDynamicOffset, indentPrefix,
                isVariableMode, isStreamMode);
        }
        else
        {
            return GetLineForDynamicArray(field, ref offset, dynamicOffset, tempDynamicOffset, indentPrefix,
                isStreamMode);
        }
    }

    private string GetLineForDynamicArray(FieldData field, ref int offset,
        StringBuilder dynamicOffset, string tempDynamicOffset, string indent, bool isStreamMode)
    {
        var sb = new StringBuilder();
        if (field.ElementTypeFullName == "System.Byte")
        {
            sb.Append(GetLineForDynamicByteArray(field, ref offset, dynamicOffset, tempDynamicOffset, isStreamMode));
        }
        else
        {
            sb.AppendLine(
                $"new {field.ElementTypeFullName}[{GetVariableNameForExpression(field.SizeFieldName!)}];");
            sb.AppendLine($"{indent}for (var i = 0; i < {GetVariableNameForExpression(field.SizeFieldName!)}; i++)");
            sb.AppendLine($"{indent}{{");
            var variableName = field.FieldName;
            if (field.ElementTypeFullName is not null &&
                field.ElementTypeFields is not null &&
                GeneratorConstants.IsCustomType(field.ElementTypeFullName))
            {
                foreach (var subField in field.ElementTypeFields)
                {
                    var line = GetMethodLine(subField, ref offset, dynamicOffset,
                        $"{tempDynamicOffset} + {field.ElementSize} * i", indent, true, isStreamMode);
                    sb.AppendLine($"{indent}    {variableName}[i].{subField.FieldName} = {line};");
                }
            }
            else
            {
                var line = GetValueForSingleValue(field, ref offset, dynamicOffset,
                    $"{tempDynamicOffset} + {field.ElementSize} * i", isStreamMode);
                sb.AppendLine($"{indent}    {variableName}[i] = {line};");
            }

            sb.Append($"{indent}}}");
        }

        dynamicOffset.Append($" + {GetVariableNameForExpression(field.SizeFieldName!)}");

        return sb.ToString();
    }

    private string GetLineForFixedArray(FieldData field, ref int offset,
        StringBuilder dynamicOffset, string tempDynamicOffset, string indentPrefix,
        bool isVariableMode, bool isStreamMode)
    {
        if (field.ElementTypeFullName == "System.Byte")
        {
            return GetLineForFixedByteArray(field, ref offset, dynamicOffset, tempDynamicOffset, isStreamMode);
        }
        else
        {
            var sb = new StringBuilder();
            sb.AppendLine("new []");
            sb.AppendLine($"{indentPrefix}{{");
            // iterate over each item in array
            for (var i = 0; i < field.ArrayLength!.Value; i++)
            {
                if (GeneratorConstants.IsCustomType(field.ElementTypeFullName!))
                {
                    var initializer = ""; // TODO
                    sb.Append($"{indentPrefix}    {initializer}");
                }
                else
                {
                    var line = GetValueForSingleValue(field, ref offset, dynamicOffset, tempDynamicOffset,
                        isStreamMode);
                    offset += field.ElementSize;
                    sb.Append($"{indentPrefix}    {line}");
                }

                if (i < field.ArrayLength.Value - 1)
                {
                    sb.AppendLine(",");
                }
                else
                {
                    sb.AppendLine();
                }
            }

            sb.Append($"{indentPrefix}}}");

            return sb.ToString();
        }
    }

    private static void ApplyHeader(StringBuilder sb, PacketTypeInfo packetTypeInfo)
    {
        sb.AppendLine("""
                      /// <auto-generated/>
                      using System;
                      using QuantumCore.Networking;

                      """);
        sb.AppendLine($"namespace {packetTypeInfo.Namespace};");
        sb.AppendLine();

        if (packetTypeInfo.StaticSize.HasValue)
        {
            sb.AppendLine($"[PacketData(StaticSize = {packetTypeInfo.StaticSize:D})]");
        }

        sb.AppendLine($"{packetTypeInfo.Modifiers} class {packetTypeInfo.Name} : IPacket");
        sb.AppendLine("{");
    }

    public static void ApplyFooter(StringBuilder source)
    {
        source.AppendLine("}");
    }
}