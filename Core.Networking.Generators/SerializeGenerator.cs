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
        var fields = _context.GetFieldsOfType(type);
        var source = new StringBuilder("        public void Serialize(byte[] bytes, int offset = 0)\r\n");
        source.AppendLine("        {");
        source.AppendLine(GenerateWriteHeader(header));
        var staticByteIndex = 1;
        foreach (var field in fields)
        {
            var line = GenerateMethodLine(field, $"this.{field.Name}", ref staticByteIndex, dynamicByteIndex, "", "            ");
            source.AppendLine(line);
        }

        source.AppendLine(@"        }");
        source.AppendLine();
        GenerateGetSizeMethod(type, source, dynamicByteIndex.ToString());

        return source.ToString();
    }

    internal string GenerateMethodLine(FieldData field, string fieldExpression, ref int offset, StringBuilder dynamicOffset, string tempDynamicOffset, string indentPrefix)
    {
        var finalLine = field switch
        {
            // handle Custom[]
            { IsArray: true } => GetLineForArray(field, fieldExpression, ref offset, dynamicOffset, tempDynamicOffset, indentPrefix),
            // handle string
            { SemanticType.Name: "String" } => GetLineForString(field, fieldExpression, ref offset, dynamicOffset, tempDynamicOffset, indentPrefix),
            // handle enum
            { IsEnum: true } => GetLineForSingleValue(field, (INamedTypeSymbol)field.SemanticType, fieldExpression, ref offset, dynamicOffset, tempDynamicOffset, indentPrefix),
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

    private static string GetLineForString(FieldData fieldData, string fieldExpression, ref int offset, StringBuilder dynamicOffset,
        string tempDynamicOffset, string indentPrefix)
    {
        var offsetStr = $"offset + {offset}{dynamicOffset}{tempDynamicOffset}";
        if (fieldData.ElementSize <= 0)
        {
            // only append to dynamic offset if type has non static length
            dynamicOffset.Append($" + {fieldExpression}.Length");
        }
        return $"{indentPrefix}System.Text.Encoding.ASCII.GetBytes({fieldExpression}).CopyTo(bytes, {offsetStr});";
    }

    private static string GetLineForFixedByteArray(FieldData field, string fieldExpression,
        ref int offset,
        StringBuilder dynamicOffset, string tempDynamicOffset, string indentPrefix)
    {
        var offsetStr = $"offset + {offset}{dynamicOffset}{tempDynamicOffset}";
        offset += field.ArrayLength!.Value;
        return $"{indentPrefix}{fieldExpression}.CopyTo(bytes, {offsetStr});";
    }

    private static string GetLineForDynamicByteArray(string fieldExpression,
        ref int offset,
        StringBuilder dynamicOffset, string tempDynamicOffset, string indentPrefix)
    {
        var offsetStr = $"offset + {offset}{dynamicOffset}{tempDynamicOffset}";
        dynamicOffset.Append($" + {fieldExpression}.Length");
        return $"{indentPrefix}{fieldExpression}.CopyTo(bytes, {offsetStr});";
    }

    private static string GetLineForSingleValue(FieldData fieldData, INamedTypeSymbol namedTypeSymbol, string fieldExpression,
        ref int offset,
        StringBuilder dynamicOffset, string tempDynamicOffset, string indentPrefix)
    {
        var cast = namedTypeSymbol.GetFullName();

        var offsetStr = $"offset + {offset}{dynamicOffset}{tempDynamicOffset}";
        if (namedTypeSymbol.TypeKind is TypeKind.Enum)
        {
            var enumUnderlyingTypeName = namedTypeSymbol.EnumUnderlyingType!.Name;
            var enumCast = namedTypeSymbol.EnumUnderlyingType.GetFullName();

            if (GeneratorConstants.SupportedTypesByBitConverter.Contains(enumUnderlyingTypeName))
            {
                return $"{indentPrefix}System.BitConverter.GetBytes(({enumCast}){fieldExpression}).CopyTo(bytes, {offsetStr});";
            }
            if (GeneratorConstants.CastableToByteTypes.Contains(enumUnderlyingTypeName))
            {
                return $"{indentPrefix}bytes[{offsetStr}] = ({enumCast}){fieldExpression};";
            }

            if (GeneratorConstants.NoCastTypes.Contains(enumUnderlyingTypeName))
            {
                return $"{indentPrefix}bytes[{offsetStr}] = (byte){fieldExpression};";
            }
        }

        if (GeneratorConstants.SupportedTypesByBitConverter.Contains(namedTypeSymbol.Name))
        {
            return $"{indentPrefix}System.BitConverter.GetBytes({fieldExpression}).CopyTo(bytes, {offsetStr});";
        }

        if (GeneratorConstants.NoCastTypes.Contains(namedTypeSymbol.Name))
        {
            return $"{indentPrefix}bytes[{offsetStr}] = {fieldExpression};";
        }

        if (GeneratorConstants.CastableToByteTypes.Contains(namedTypeSymbol.Name))
        {
            return $"{indentPrefix}bytes[{offsetStr}] = ({cast}){fieldExpression};";
        }

        if (namedTypeSymbol.GetFullName() == "System.String")
        {
            return GetLineForString(fieldData, fieldExpression, ref offset, dynamicOffset, tempDynamicOffset, indentPrefix);
        }

        throw new InvalidOperationException($"Don't know how to handle type {namedTypeSymbol.Name}");
    }

    private string GenerateLineForMisc(FieldData field, string fieldExpression, ref int offset, StringBuilder dynamicOffset,
        string tempDynamicOffset, string indentPrefix = "")
    {
        if (field.IsCustom)
        {
            // handle custom type
            var fieldTypeFullName = field.SemanticType.GetFullName();
            if (!_context.RelevantTypes.TryGetValue(fieldTypeFullName!, out var type))
            {
                throw new InvalidOperationException(
                    $"Could not find type declaration for type {fieldTypeFullName}");
            }

            var subFields = _context.GetFieldsOfType(type.TypeDeclaration);
            var lines = new List<string>();
            foreach (var subField in subFields)
            {
                var subLine = GenerateMethodLine(subField, $"{fieldExpression}.{subField.Name}", ref offset, dynamicOffset, tempDynamicOffset, indentPrefix);
                lines.Add(subLine);
            }

            return string.Join("\r\n", lines);
        }
        else if (field.SemanticType is INamedTypeSymbol namedTypeSymbol)
        {
            return GetLineForSingleValue(field, namedTypeSymbol, fieldExpression, ref offset, dynamicOffset, tempDynamicOffset, indentPrefix);
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
                return GetLineForFixedArray(field, fieldExpression, ref offset, dynamicOffset, tempDynamicOffset, indentPrefix, arr);
            }
            else
            {
                return GetLineForDynamicArray(field, fieldExpression, ref offset, dynamicOffset, indentPrefix, arr);
            }
        }

        throw new NotImplementedException(
            $"Don't know how to handle array of {((IArrayTypeSymbol)field.SemanticType).ElementType}");
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
            var lines = new List<string>
            {
                $"{indentPrefix}for (var i = 0; i < {fieldExpression}.Length; i++)",
                $"{indentPrefix}{{"
            };

            if (GeneratorContext.IsCustomType(arr.ElementType))
            {
                if (!_context.RelevantTypes.TryGetValue(subTypeFullName, out var subType))
                {
                    throw new InvalidOperationException($"Could not find required type {subTypeFullName}");
                }

                // recursive call to generate lines for each field in sub type
                var subTypes = _context.GetFieldsOfType(subType.TypeDeclaration);

                for (var ii = 0; ii < subTypes.Count; ii++)
                {
                    var member = subTypes[ii];
                    var subFieldExpression = $"{fieldExpression}[i].{member.Name}";
                    var line = GenerateMethodLine(member, subFieldExpression, ref offset, dynamicOffset,
                        $" + i * {member.ElementSize}", $"{indentPrefix}    ");
                    lines.Add(line);
                }
                dynamicOffset.Append($" + {fieldExpression}.Length * {subTypes.Sum(x => x.ElementSize)}");
            }
            else
            {
                var elementType = (INamedTypeSymbol)((IArrayTypeSymbol)field.SemanticType).ElementType;
                var subFieldExpression = $"{fieldExpression}[i]";
                var line = GetLineForSingleValue(field, elementType, subFieldExpression, ref offset, dynamicOffset, 
                    $" + i * {field.ElementSize}", $"{indentPrefix}    ");
                dynamicOffset.Append($" + {fieldExpression}.Length * {field.ElementSize}");
                lines.Add(line);
            }

            lines.Add($"{indentPrefix}}}");

            return string.Join("\r\n", lines);
        }
    }

    private string GetLineForFixedArray(FieldData field, string fieldExpression, ref int offset,
        StringBuilder dynamicOffset, string tempDynamicOffset, string indentPrefix, IArrayTypeSymbol arr)
    {
        var subTypeFullName = arr.ElementType.GetFullName()!;
        var lines = new List<string>();
        
        if (subTypeFullName == "System.Byte")
        {
            return GetLineForFixedByteArray(field, fieldExpression, ref offset, dynamicOffset, tempDynamicOffset, indentPrefix);
        }
        else
        {
            // iterate over each item in array
            for (var i = 0; i < field.ArrayLength!.Value; i++)
            {
                if (GeneratorContext.IsCustomType(arr.ElementType))
                {
                    if (!_context.RelevantTypes.TryGetValue(subTypeFullName, out var subType))
                    {
                        throw new InvalidOperationException($"Could not find required type {subTypeFullName}");
                    }

                    // recursive call to generate lines for each field in sub type
                    var members = _context.GetFieldsOfType(subType.TypeDeclaration);
                    for (var ii = 0; ii < members.Count; ii++)
                    {
                        var member = members[ii];
                        var line = GenerateMethodLine(member, $"{fieldExpression}[{i}].{member.Name}", ref offset,
                            dynamicOffset,
                            tempDynamicOffset, indentPrefix);
                        lines.Add(line);
                    }
                }
                else
                {
                    var subFieldExpression = $"{fieldExpression}[{i}]";
                    var elementType = (INamedTypeSymbol)((IArrayTypeSymbol)field.SemanticType).ElementType;
                    var line = GetLineForSingleValue(field, elementType, subFieldExpression, ref offset, dynamicOffset,
                        tempDynamicOffset, indentPrefix);
                    offset += field.ElementSize;
                    lines.Add(line);
                }
            }
        }

        return string.Join("\r\n", lines);
    }

    private void GenerateGetSizeMethod(TypeDeclarationSyntax type, StringBuilder sb, string dynamicSize)
    {
        var fields = _context.GetFieldsOfType(type);
        var size = GeneratorContext.GetStaticSizeOfType(fields) + 1; // + header size
        sb.AppendLine("        public ushort GetSize()");
        sb.AppendLine("        {");

        var body = !string.IsNullOrWhiteSpace(dynamicSize)
            ? $"            return (ushort)({size}{(!string.IsNullOrWhiteSpace(dynamicSize) ? dynamicSize : "")});"
            : $"            return {size.ToString()};";
        sb.AppendLine(body);

        sb.AppendLine("        }");
    }

    private static string GenerateWriteHeader(string header)
    {
        return $"            bytes[offset + 0] = {header};";
    }
}