﻿using System.Text;
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
        var source = new StringBuilder("        public void Serialize(byte[] bytes, in int offset = 0)\r\n");
        source.AppendLine("        {");
        source.AppendLine(GenerateWriteHeader(header, subHeader));
        var staticByteIndex = subHeader is not null ? 2 : 1;
        foreach (var field in fields.ToArray())
        {
            var fieldExpression = fields.Any(x => x.SizeFieldName == field.Name) 
                ? "this.GetSize()" 
                : $"this.{field.Name}";

            var line = GenerateMethodLine(field, fieldExpression, ref staticByteIndex, dynamicByteIndex, "", "            ");
            source.AppendLine(line);
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

        var lengthString = fieldData.SizeFieldName is not null
            ? $"this.{fieldData.SizeFieldName} + 1"
            : fieldData.ElementSize.ToString();
        return $"{indentPrefix}bytes.WriteString({fieldExpression}, {offsetStr}, (int){lengthString});";
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

    private string GetLineForSingleValue(FieldData fieldData, INamedTypeSymbol namedTypeSymbol, string fieldExpression,
        ref int offset,
        StringBuilder dynamicOffset, string tempDynamicOffset, string indentPrefix)
    {
        var offsetStr = $"offset + {offset}{dynamicOffset}{tempDynamicOffset}";
        var type = namedTypeSymbol.TypeKind is TypeKind.Enum
            ? namedTypeSymbol.EnumUnderlyingType!.Name
            : namedTypeSymbol.Name;
        var cast = namedTypeSymbol.TypeKind is TypeKind.Enum
            ? $"({namedTypeSymbol.EnumUnderlyingType.GetFullName()})"
            : GeneratorConstants.CastableToByteTypes.Contains(namedTypeSymbol.Name)
                ? $"({namedTypeSymbol.GetFullName()})"
                : "";

        if (GeneratorConstants.SupportedTypesByBitConverter.Contains(type))
        {
            if (type is "Int32" or "UInt32" or 
                        "Int16" or "UInt16" or 
                        "Int64" or "UInt64")
            {
                var sb = new StringBuilder();
                for (int i = 0; i < fieldData.ElementSize; i++)
                {
                    var offsetStrLocal = $"offset + {offset + i}{dynamicOffset}{tempDynamicOffset}";
                    var line = $"{indentPrefix}bytes[{offsetStrLocal}] = (byte)({cast}{fieldExpression} >> {8 * i});";

                    if (i < fieldData.ElementSize - 1)
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
                return $"{indentPrefix}System.BitConverter.GetBytes({cast}{fieldExpression}).CopyTo(bytes, {offsetStr});";
            }
        }
        if (GeneratorConstants.CastableToByteTypes.Contains(type))
        {
            return $"{indentPrefix}bytes[{offsetStr}] = {cast}{fieldExpression};";
        }

        if (GeneratorConstants.NoCastTypes.Contains(type))
        {
            return $"{indentPrefix}bytes[{offsetStr}] = {cast}{fieldExpression};";
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
            var type = _context.GetTypeDeclaration(field.SemanticType);
            var subFields = _context.GetFieldsOfType(type);
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
                // recursive call to generate lines for each field in sub type
                var subType = _context.GetTypeDeclaration(arr.ElementType);
                var subTypes = _context.GetFieldsOfType(subType);

                for (var ii = 0; ii < subTypes.Count; ii++)
                {
                    var member = subTypes[ii];
                    var subFieldExpression = $"{fieldExpression}[i].{member.Name}";
                    var line = GenerateMethodLine(member, subFieldExpression, ref offset, dynamicOffset,
                        $" + i * {field.ElementSize}", $"{indentPrefix}    ");
                    lines.Add(line);
                }
                offset -= field.ElementSize; // reduce the offset after the array to make the offset correct
                dynamicOffset.Append($" + {fieldExpression}.Length * {field.ElementSize}");
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
                    // recursive call to generate lines for each field in sub type
                    var subType = _context.GetTypeDeclaration(arr.ElementType);
                    var members = _context.GetFieldsOfType(subType);
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

    private void GenerateGetSizeMethod(TypeDeclarationSyntax type, StringBuilder sb, string dynamicSize, bool hasSubHeader, bool hasSequence)
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

    private static string GenerateWriteHeader(string header, string? subHeader)
    {
        var sb = new StringBuilder();
        sb.Append($"            bytes[offset + 0] = {header};");
        if (subHeader is not null)
        {
            sb.AppendLine();
            sb.Append($"            bytes[offset + 1] = {subHeader};");
        }

        return sb.ToString();
    }
}