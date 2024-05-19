using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace QuantumCore.Networking;

internal class SerializeGenerator
{
    private readonly GeneratorContext _context;

    public SerializeGenerator(GeneratorContext context)
    {
        _context = context;
    }

    public string Generate(TypeDeclarationSyntax type, StringBuilder dynamicByteIndex)
    {
        var header = _context.GetHeaderForType(type);
        var subHeader = _context.GetSubHeaderForType(type);
        var hasSequence = _context.HasTypeSequence(type);
        var fields = _context.GetFieldsOfType(type);
        var source = new StringBuilder();
        source.AppendLine("        public void Serialize(byte[] bytes, in int offset = 0)");
        source.AppendLine("        {");
        source.AppendLine(GenerateWriteHeader(header));
        var staticByteIndex = 1;
        if (fields.Length == 0 && subHeader is not null)
        {
            source.AppendLine(
                $"            bytes[offset + 1] = 0x{Convert.ToString(subHeader.Value.Value, 16).PadLeft(2, '0')};");
            staticByteIndex++;
        }
        else
        {
            for (var index = 0; index < fields.Length; index++)
            {
                var field = fields[index];
                var fieldExpression = fields.Any(x => x.SizeFieldName == field.Name)
                    ? "this.GetSize()"
                    : $"this.{field.Name}";

                if (subHeader is not null && subHeader.Value.Position == index)
                {
                    source.AppendLine(
                        $"            bytes[offset + {staticByteIndex}] = 0x{Convert.ToString(subHeader.Value.Value, 16).PadLeft(2, '0')};");
                    staticByteIndex++;
                }

                var line = GenerateMethodLine(field, fieldExpression, ref staticByteIndex, dynamicByteIndex, "",
                    "            ");
                source.AppendLine(line);
            }
        }

        if (hasSequence)
        {
            source.AppendLine($"            bytes[offset + {staticByteIndex}{dynamicByteIndex}] = default;");
        }

        source.AppendLine("        }");
        source.AppendLine();
        GenerateGetSizeMethod(type, source, dynamicByteIndex.ToString(), subHeader is not null, hasSequence);

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
            {SemanticType.Name: "String"} => GetLineForString(field, fieldExpression, ref offset, dynamicOffset,
                tempDynamicOffset, indentPrefix),
            // handle enum
            {IsEnum: true} => GetLineForSingleValue(field, (INamedTypeSymbol) field.SemanticType, fieldExpression,
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
        var offsetStr = $"offset + {offset}{dynamicOffset}{tempDynamicOffset}";
        if (fieldData.HasDynamicLength)
        {
            // only append to dynamic offset if type has non static length
            dynamicOffset.Append($" + {fieldExpression}.Length");
        }

        var lengthString = fieldData.HasDynamicLength
            ? $"(int)this.{fieldData.SizeFieldName} + 1"
            : fieldData.ElementSize.ToString();
        return $"{indentPrefix}bytes.WriteString({fieldExpression}, {offsetStr}, {lengthString});";
    }

    internal static string GetLineForFixedByteArray(FieldData field, string fieldExpression,
        ref int offset,
        StringBuilder dynamicOffset, string tempDynamicOffset, string indentPrefix)
    {
        var offsetStr = $"offset + {offset}{dynamicOffset}{tempDynamicOffset}";
        offset += field.ArrayLength!.Value;
        return $"{indentPrefix}{fieldExpression}.CopyTo(bytes, {offsetStr});";
    }

    internal static string GetLineForDynamicByteArray(string fieldExpression,
        ref int offset,
        StringBuilder dynamicOffset, string tempDynamicOffset, string indentPrefix)
    {
        var offsetStr = $"offset + {offset}{dynamicOffset}{tempDynamicOffset}";
        dynamicOffset.Append($" + {fieldExpression}.Length");
        return $"{indentPrefix}{fieldExpression}.CopyTo(bytes, {offsetStr});";
    }

    internal static string GetLineForSingleValue(FieldData fieldData, INamedTypeSymbol namedTypeSymbol, string fieldExpression,
        ref int offset,
        StringBuilder dynamicOffset, string tempDynamicOffset, string indentPrefix)
    {
        var offsetStr = $"offset + {offset}{dynamicOffset}{tempDynamicOffset}";
        var type = fieldData.IsEnum
            ? namedTypeSymbol.EnumUnderlyingType!.Name
            : namedTypeSymbol.Name;
        var cast = fieldData.IsEnum
            ? $"({namedTypeSymbol.EnumUnderlyingType!.GetFullName()})"
            : "";

        if (GeneratorConstants.SupportedTypesByBitConverter.Contains(type) || GeneratorConstants.ConvertTypes.Contains(type))
        {
            if (type is "Int32" or "UInt32" or
                "Int16" or "UInt16" or
                "Int64" or "UInt64")
            {
                var sb = new StringBuilder();
                var elementSize = GeneratorConstants.GetSizeOfPrimitiveType(type);
                for (int i = 0; i < elementSize; i++)
                {
                    var offsetStrLocal = $"offset + {offset + i}{dynamicOffset}{tempDynamicOffset}";
                    var line = $"{indentPrefix}bytes[{offsetStrLocal}] = (System.Byte)({cast}{fieldExpression} >> {8 * i});";

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
                    $"{indentPrefix}System.BitConverter.GetBytes({cast}{fieldExpression}).CopyTo(bytes, {offsetStr});";
            }
        }

        if (GeneratorConstants.NoCastTypes.Contains(type))
        {
            return fieldData.IsEnum
                ? $"{indentPrefix}bytes[{offsetStr}] = {cast}{fieldExpression};"
                : $"{indentPrefix}bytes[{offsetStr}] = {fieldExpression};";
        }

        if (type == "String")
        {
            return GetLineForString(fieldData, fieldExpression, ref offset, dynamicOffset, tempDynamicOffset,
                indentPrefix);
        }

        if (namedTypeSymbol.GetFullName() == "System.Boolean")
        {
            return $"{indentPrefix}bytes[{offsetStr}] = (byte)({fieldExpression} ? 1 : 0);";
        }

        throw new InvalidOperationException($"Don't know how to handle type {namedTypeSymbol.Name}");
    }

    private string GenerateLineForMisc(FieldData field, string fieldExpression, ref int offset,
        StringBuilder dynamicOffset,
        string tempDynamicOffset, string indentPrefix = "")
    {
        if (field.IsCustom)
        {
            // handle custom type
            var type = _context.GetTypeDeclaration(field.SemanticType);
            var subFields = _context.GetFieldsOfType(type);
            var lines = new StringBuilder();
            for (var i = 0; i < subFields.Length; i++)
            {
                var subField = subFields[i];
                var subLine = GenerateMethodLine(subField, $"{fieldExpression}.{subField.Name}", ref offset,
                    dynamicOffset, tempDynamicOffset, indentPrefix);

                if (i < subFields.Length - 1)
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
        else if (field.SemanticType is INamedTypeSymbol namedTypeSymbol)
        {
            return GetLineForSingleValue(field, namedTypeSymbol, fieldExpression, ref offset, dynamicOffset,
                tempDynamicOffset, indentPrefix);
        }

        throw new NotImplementedException("???");
    }

    private string GetLineForArray(FieldData field, string fieldExpression, ref int offset, StringBuilder dynamicOffset,
        string tempDynamicOffset, string indentPrefix)
    {
        if (field.SemanticType is IArrayTypeSymbol arr)
        {
            if (field.ArrayLength.HasValue)
            {
                return GetLineForFixedArray(field, fieldExpression, ref offset, dynamicOffset, tempDynamicOffset,
                    indentPrefix, arr);
            }
            else
            {
                return GetLineForDynamicArray(field, fieldExpression, ref offset, dynamicOffset, indentPrefix, arr);
            }
        }

        throw new NotImplementedException(
            $"Don't know how to handle array of {((IArrayTypeSymbol) field.SemanticType).ElementType}");
    }

    private string GetLineForDynamicArray(FieldData field, string fieldExpression, ref int offset,
        StringBuilder dynamicOffset,
        string indentPrefix, IArrayTypeSymbol arr)
    {
        var subTypeFullName = arr.ElementType.GetFullName()!;
        if (subTypeFullName == "System.Byte")
        {
            return GetLineForDynamicByteArray(fieldExpression, ref offset, dynamicOffset, "", indentPrefix);
        }
        else
        {
            var lines = new StringBuilder();
            lines.AppendLine($"{indentPrefix}for (var i = 0; i < {fieldExpression}.Length; i++)");
            lines.AppendLine($"{indentPrefix}{{");

            if (GeneratorContext.IsCustomType(arr.ElementType))
            {
                // recursive call to generate lines for each field in sub type
                var subType = _context.GetTypeDeclaration(arr.ElementType);
                var subTypes = _context.GetFieldsOfType(subType);

                for (var ii = 0; ii < subTypes.Length; ii++)
                {
                    var member = subTypes[ii];
                    var subFieldExpression = $"{fieldExpression}[i].{member.Name}";
                    var line = GenerateMethodLine(member, subFieldExpression, ref offset, dynamicOffset,
                        $" + i * {field.ElementSize}", $"{indentPrefix}    ");
                    lines.AppendLine(line);
                }

                offset -= field.ElementSize; // reduce the offset after the array to make the offset correct
                dynamicOffset.Append($" + {fieldExpression}.Length * {field.ElementSize}");
            }
            else
            {
                var elementType = (INamedTypeSymbol) ((IArrayTypeSymbol) field.SemanticType).ElementType;
                var subFieldExpression = $"{fieldExpression}[i]";
                var line = GetLineForSingleValue(field, elementType, subFieldExpression, ref offset, dynamicOffset,
                    $" + i * {field.ElementSize}", $"{indentPrefix}    ");
                dynamicOffset.Append($" + {fieldExpression}.Length * {field.ElementSize}");
                lines.AppendLine(line);
            }

            lines.Append($"{indentPrefix}}}");
            return lines.ToString();
        }
    }

    private string GetLineForFixedArray(FieldData field, string fieldExpression, ref int offset,
        StringBuilder dynamicOffset, string tempDynamicOffset, string indentPrefix, IArrayTypeSymbol arr)
    {
        var subTypeFullName = arr.ElementType.GetFullName()!;
        var lines = new StringBuilder();

        if (subTypeFullName == "System.Byte")
        {
            return GetLineForFixedByteArray(field, fieldExpression, ref offset, dynamicOffset, tempDynamicOffset,
                indentPrefix);
        }
        else
        {
            // iterate over each item in array
            for (var i = 0; i < field.ArrayLength!.Value; i++)
            {
                if (GeneratorContext.IsCustomType(arr.ElementType))
                {
                    // recursive call to generate lines for each field in sub type
                    var subType = _context.GetTypeDeclaration(arr.ElementType);
                    var members = _context.GetFieldsOfType(subType);
                    for (var ii = 0; ii < members.Length; ii++)
                    {
                        var member = members[ii];
                        var line = GenerateMethodLine(member, $"{fieldExpression}[{i}].{member.Name}", ref offset,
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
                    var elementType = (INamedTypeSymbol) ((IArrayTypeSymbol) field.SemanticType).ElementType;
                    var line = GetLineForSingleValue(field, elementType, subFieldExpression, ref offset, dynamicOffset,
                        tempDynamicOffset, indentPrefix);
                    offset += field.ElementSize;
                    lines.AppendLine(line);
                }
            }
        }

        return lines.ToString().TrimEnd();
    }

    private void GenerateGetSizeMethod(TypeDeclarationSyntax type, StringBuilder sb, string dynamicSize,
        bool hasSubHeader, bool hasSequence)
    {
        var fields = _context.GetFieldsOfType(type);
        var size = GeneratorContext.GetStaticSizeOfType(fields) + 1; // + header
        if (hasSubHeader)
        {
            size++;
        }

        if (hasSequence)
        {
            size++;
        }

        sb.AppendLine("        public ushort GetSize()");
        sb.AppendLine("        {");

        var dynamicString = !string.IsNullOrWhiteSpace(dynamicSize)
            ? dynamicSize
            : "";

        if (fields.Any(x => x.SemanticType.Name == "String" && x.HasDynamicLength))
        {
            dynamicString = $"{dynamicString} + 1";
        }

        var body = dynamicString != ""
            ? $"            return (ushort)({size}{dynamicString});"
            : $"            return {size.ToString()};";
        sb.AppendLine(body);

        sb.AppendLine("        }");
    }

    internal static string GenerateWriteHeader(string header)
    {
        return $"            bytes[offset + 0] = {header};";
    }
}
