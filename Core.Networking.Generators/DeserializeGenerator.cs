using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace QuantumCore.Networking;

internal class DeserializeGenerator
{
    private readonly GeneratorContext _context;
    private static readonly Regex TrimAssignmentRegex = new(@"(\s+) = (.+)", RegexOptions.Compiled);

    public DeserializeGenerator(GeneratorContext context)
    {
        _context = context;
    }

    public string Generate(TypeDeclarationSyntax type, string dynamicByteIndex)
    {
        var fields = _context.GetFieldsOfType(type);
        var source = new StringBuilder($"        public static {type.Identifier.Text} Deserialize(byte[] bytes, int offset = 0)\r\n");
        source.AppendLine("        {");
        var staticByteIndex = 0;
        var dynamicByteIndexLocal = new StringBuilder(dynamicByteIndex);
        const string indentPrefix = "            ";
        // declare and initialize variables
        foreach (var field in fields)
        {
            var line = GetMethodLine(field, ref staticByteIndex, dynamicByteIndexLocal, "", indentPrefix, true);
            source.Append($"{indentPrefix}var {GetVariableNameForExpression(field.Name)} = {line}");
            if (field is not { IsArray: true, HasDynamicLength: true } || (field.IsArray && (field.SemanticType as IArrayTypeSymbol)?.ElementType.Name == "Byte"))
            {
                // dynamic arrays have a for loop after their declaration so don't put a ;
                source.AppendLine(";");
            }
            else
            {
                source.AppendLine();
            }
        }
        
        // apply variables
        dynamicByteIndexLocal = new StringBuilder(dynamicByteIndex);
        if (type is RecordDeclarationSyntax)
        {
            var line = GetLineForInitializer(_context.GetTypeInfo(type)!, ref staticByteIndex, dynamicByteIndexLocal, "", indentPrefix, false);
            source.AppendLine($"{indentPrefix}var obj = {line};");
        }
        else
        {
            source.AppendLine($"{indentPrefix}var obj = new {_context.GetTypeInfo(type).GetFullName()}");
            source.AppendLine($"{indentPrefix}{{");
            for (var i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                if (field.IsReadonly) continue;
                var line = $"{indentPrefix}    {field.Name} = {GetVariableNameForExpression(field.Name)}";
                source.Append(line);

                if (i < fields.Count - 1)
                {
                    source.AppendLine(",");
                }
                else
                {
                    source.AppendLine();
                }
            }

            source.AppendLine($"{indentPrefix}}};");
        }
        source.AppendLine($"{indentPrefix}return obj;");
        source.AppendLine("        }");
        source.AppendLine();
        source.AppendLine(@"        public static T Deserialize<T>(byte[] bytes, int offset = 0)
            where T : IPacketSerializable
        {
            return (T)(object)Deserialize(bytes, offset);
        }");

        return source.ToString();
    }

    internal string GetMethodLine(FieldData field, ref int offset, StringBuilder dynamicOffset, string tempDynamicOffset, string indentPrefix, bool isVariableMode)
    {
        var finalLine = field switch
        {
            // handle Custom[]
            { IsArray: true } => GetLineForArray(field, ref offset, dynamicOffset, tempDynamicOffset, indentPrefix, isVariableMode),
            // handle string
            { SemanticType.Name: "String" } => GetValueForString(field, ref offset, dynamicOffset, tempDynamicOffset),
            // handle enum
            { IsEnum: true } => GetValueForSingleValue(field, (INamedTypeSymbol)field.SemanticType, ref offset, dynamicOffset, tempDynamicOffset),
            // misc
            _ => GetLineForMisc(field, ref offset, dynamicOffset, tempDynamicOffset, indentPrefix, isVariableMode)
        };


        if (!field.IsArray)
        {
            offset += field.ElementSize;
        }

        // handle anything else
        return finalLine;
    }

    private static string GetValueForString(FieldData fieldData, ref int offset, StringBuilder dynamicOffset,
        string tempDynamicOffset)
    {
        var offsetStr = GetOffsetString(offset, dynamicOffset, tempDynamicOffset);
        if (fieldData.SizeFieldName is not null)
        {
            var variableName = GetVariableNameForExpression(fieldData.SizeFieldName);
            dynamicOffset.Append($" + {variableName}");
        }
        var endOffsetStr = GetOffsetString(offset, dynamicOffset, fieldData.ElementSize > 0 
            ? $"{tempDynamicOffset} + {fieldData.ElementSize}" 
            : tempDynamicOffset);
        return $"System.Text.Encoding.ASCII.GetString(bytes[{offsetStr}..{endOffsetStr}])";
    }

    private static string GetOffsetString(in int offset, StringBuilder dynamicOffset, string tempDynamicOffset, int? elementSize = null)
    {
        if (elementSize.HasValue)
        {
            if (dynamicOffset.Length > 0)
            {
                return $"(System.Index)(offset + {offset}{dynamicOffset}{tempDynamicOffset})..(System.Index)(offset + {offset}{dynamicOffset}{tempDynamicOffset} + {elementSize})";
            }
            else
            {
                return $"(offset + {offset}{tempDynamicOffset})..(offset + {offset}{tempDynamicOffset} + {elementSize})";
            }
        }
        else
        {
            if (dynamicOffset.Length > 0)
            {
                return $"(System.Index)(offset + {offset}{dynamicOffset}{tempDynamicOffset})";
            }
            else
            {
                return $"(offset + {offset}{tempDynamicOffset})";
            }
        }
    }

    private static string GetLineForFixedByteArray(FieldData field, ref int offset, StringBuilder dynamicOffset, string tempDynamicOffset)
    {
        var offsetStr = GetOffsetString(offset, dynamicOffset, tempDynamicOffset);
        var endOffsetStr = GetOffsetString(offset, dynamicOffset, $"{tempDynamicOffset} + {field.ArrayLength!.Value}");
        offset += field.ArrayLength!.Value;
        return $"bytes[{offsetStr}..{endOffsetStr}]";
    }

    private string GetLineForDynamicByteArray(FieldData field, ref int offset,
        StringBuilder dynamicOffset, string tempDynamicOffset)
    {
        var offsetStr = GetOffsetString(offset, dynamicOffset, tempDynamicOffset);
        dynamicOffset.Append($" + {GetVariableNameForExpression(field.SizeFieldName!)}");
        var endOffsetStr = GetOffsetString(offset, dynamicOffset, tempDynamicOffset);
        return $"bytes[{offsetStr}..{endOffsetStr}]";
    }

    private static string GetVariableNameForExpression(string fieldExpression)
    {
        return $"__{fieldExpression.Replace(".", "_")}";
    }

    private static string GetValueForSingleValue(FieldData field, INamedTypeSymbol namedTypeSymbol, ref int offset,
        StringBuilder dynamicOffset, string tempDynamicOffset)
    {
        var cast = namedTypeSymbol.GetFullName();
        var typeName = namedTypeSymbol.TypeKind is TypeKind.Enum
            ? namedTypeSymbol.EnumUnderlyingType!.Name
            : namedTypeSymbol.Name;
        int? elementSize = GeneratorConstants.SupportedTypesByBitConverter.Contains(typeName)
            ? GetSizeOfPrimitiveType(typeName) 
            :null;
        var offsetStr = GetOffsetString(offset, dynamicOffset, tempDynamicOffset, elementSize);
        if (namedTypeSymbol.TypeKind is TypeKind.Enum)
        {
            var enumUnderlyingTypeName = namedTypeSymbol.EnumUnderlyingType!.Name;

            if (GeneratorConstants.SupportedTypesByBitConverter.Contains(enumUnderlyingTypeName))
            {
                return $"({cast})System.BitConverter.To{namedTypeSymbol.EnumUnderlyingType.Name}(bytes[{offsetStr}])";
            }
            if (GeneratorConstants.CastableToByteTypes.Contains(enumUnderlyingTypeName) ||
                GeneratorConstants.NoCastTypes.Contains(enumUnderlyingTypeName))
            {
                return $"({cast})bytes[{offsetStr}]";
            }
        }

        if (GeneratorConstants.SupportedTypesByBitConverter.Contains(namedTypeSymbol.Name))
        {
            return $"System.BitConverter.To{namedTypeSymbol.Name}(bytes[{offsetStr}])";
        }

        if (GeneratorConstants.NoCastTypes.Contains(namedTypeSymbol.Name))
        {
            return $"bytes[{offsetStr}]";
        }

        if (GeneratorConstants.CastableToByteTypes.Contains(namedTypeSymbol.Name))
        {
            return $"({cast})bytes[{offsetStr}]";
        }

        if (namedTypeSymbol.Name == "String")
        {
            return GetValueForString(field, ref offset, dynamicOffset, tempDynamicOffset);
        }

        throw new InvalidOperationException($"Don't know how to handle type {namedTypeSymbol.Name}");
    }

    private string GetLineForMisc(FieldData field, ref int offset, StringBuilder dynamicOffset,
        string tempDynamicOffset, string indentPrefix, bool isVariableMode)
    {
        if (field.IsCustom)
        {
            // handle custom type
            return GetLineForInitializer(field.SemanticType, ref offset, dynamicOffset, tempDynamicOffset,
                indentPrefix, isVariableMode);
        }
        else if (field.SemanticType is INamedTypeSymbol namedTypeSymbol)
        {
            return GetValueForSingleValue(field, namedTypeSymbol, ref offset, dynamicOffset, tempDynamicOffset);
        }

        throw new NotImplementedException("???");
    }

    private string GetLineForArray(FieldData field, ref int offset, StringBuilder dynamicOffset,
        string tempDynamicOffset, string indentPrefix, bool isVariableMode)
    {
        if (field.SemanticType is not IArrayTypeSymbol arr)
        {
            throw new InvalidOperationException($"Called array method on a non array field");
        }

        if (!isVariableMode)
        {
            return GetVariableNameForExpression(field.Name);
        }
        if (field.ArrayLength.HasValue)
        {
            return GetLineForFixedArray(field, ref offset, dynamicOffset, tempDynamicOffset, arr, indentPrefix, isVariableMode);
        }
        else
        {
            return GetLineForDynamicArray(field, ref offset, dynamicOffset, tempDynamicOffset, arr, indentPrefix);
        }

    }

    private string GetLineForDynamicArray(FieldData field, ref int offset,
        StringBuilder dynamicOffset, string tempDynamicOffset, IArrayTypeSymbol arr, string indent)
    {
        var sb = new StringBuilder();
        if (arr.ElementType.Name == "Byte")
        {
            sb.Append(GetLineForDynamicByteArray(field, ref offset, dynamicOffset, tempDynamicOffset));
        }
        else
        {
            sb.AppendLine($"new {arr.ElementType.GetFullName()}[{GetVariableNameForExpression(field.SizeFieldName!)}];");
            sb.AppendLine($"{indent}for (var i = 0; i < {GetVariableNameForExpression(field.SizeFieldName!)}; i++)");
            sb.AppendLine($"{indent}{{");
            var variableName = GetVariableNameForExpression(field.Name);
            if (GeneratorContext.IsCustomType(arr.ElementType))
            {
                var type = _context.GetTypeDeclaration(arr.ElementType);
                var subFields = _context.GetFieldsOfType(type);
                foreach (var subField in subFields)
                {
                    var line = GetMethodLine(subField, ref offset, dynamicOffset, $"{tempDynamicOffset} + {field.ElementSize} * i", indent, true);
                    sb.AppendLine($"{indent}    {variableName}[i].{subField.Name} = {line};");
                }
            }
            else
            {
                var line = GetValueForSingleValue(field, (INamedTypeSymbol)arr.ElementType, ref offset, dynamicOffset, $"{tempDynamicOffset} + {field.ElementSize} * i");
                sb.AppendLine($"{indent}    {variableName}[i] = {line};");
            }
            sb.Append($"{indent}}}");
        }
        dynamicOffset.Append($" + {GetVariableNameForExpression(field.SizeFieldName!)}");

        return sb.ToString();
    }

    private string GetLineForFixedArray(FieldData field, ref int offset,
        StringBuilder dynamicOffset, string tempDynamicOffset, IArrayTypeSymbol arr, string indentPrefix, bool isVariableMode)
    {
        var subTypeFullName = arr.ElementType.GetFullName()!;
        
        if (subTypeFullName == "System.Byte")
        {
            return GetLineForFixedByteArray(field, ref offset, dynamicOffset, tempDynamicOffset);
        }
        else
        {
            var sb = new StringBuilder();
            sb.AppendLine("new []");
            sb.AppendLine($"{indentPrefix}{{");
            // iterate over each item in array
            for (var i = 0; i < field.ArrayLength!.Value; i++)
            {
                if (GeneratorContext.IsCustomType(arr.ElementType))
                {
                    var initializer = GetLineForInitializer(arr.ElementType, ref offset, dynamicOffset, tempDynamicOffset, $"    {indentPrefix}", isVariableMode);
                    sb.Append($"{indentPrefix}    {initializer}");
                }
                else
                {
                    var elementType = (INamedTypeSymbol)((IArrayTypeSymbol)field.SemanticType).ElementType;
                    var line = GetValueForSingleValue(field, elementType, ref offset, dynamicOffset, tempDynamicOffset);
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

    private string GetLineForInitializer(ITypeSymbol t, ref int offset, StringBuilder dynamicOffset, string tempDynamicOffset, string indentPrefix, bool isVariableMode)
    {
        var sb = new StringBuilder();

        var type = _context.GetTypeDeclaration(t);
        sb.Append($"new {_context.GetTypeInfo(type).GetFullName()}");
        
        // recursive call to generate lines for each field in sub type
        var members = _context.GetFieldsOfType(type);
        var recordParamMembers = members.Where(x => x.IsRecordParameter).ToArray();
        var propertyMembers = members.Where(x => !x.IsReadonly && !x.IsRecordParameter).ToArray();
        if (recordParamMembers.Length > 0)
        {
            sb.AppendLine("(");
            for (var ii = 0; ii < recordParamMembers.Length; ii++)
            {
                var member = recordParamMembers[ii];
                var valueStr = isVariableMode 
                    ? GetMethodLine(member, ref offset, dynamicOffset, tempDynamicOffset, indentPrefix, isVariableMode)
                    : GetVariableNameForExpression(member.Name);
                var line = $"{indentPrefix}    {valueStr}";
                line = TrimAssignmentRegex.Replace(line, "$1$2");
                sb.Append(line);

                if (ii < recordParamMembers.Length - 1)
                {
                    sb.AppendLine(",");
                }
                else
                {
                    sb.AppendLine();
                }
            }

            sb.Append($"{indentPrefix})");
        }

        if (propertyMembers.Length > 0)
        {
            sb.AppendLine();
            sb.AppendLine($"{indentPrefix}{{");
            for (var ii = 0; ii < propertyMembers.Length; ii++)
            {
                var member = propertyMembers[ii];
                var valueStr = isVariableMode 
                    ? GetMethodLine(member, ref offset, dynamicOffset, tempDynamicOffset, indentPrefix, isVariableMode)
                    : GetVariableNameForExpression(member.Name);
                var line = $"{indentPrefix}    {member.Name} = {valueStr}";
                
                sb.Append(line);
                if (ii < propertyMembers.Length - 1)
                {
                    sb.AppendLine(",");
                }
                else
                {
                    sb.AppendLine();
                }
            }

            sb.Append($"{indentPrefix}}}");
        }

        return sb.ToString();
    }

    private static int GetSizeOfPrimitiveType(string name)
    {
        return name switch
        {
            "Int64" or "UInt64" or "Double" => 2,
            "Int32" or "UInt32" or "Single" => 4,
            "Int16" or "UInt16" or "Half" => 2,
            "Byte" or "SByte" or "Boolean" => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(name), $"Don't know the size of {name}")
        };
    }
}