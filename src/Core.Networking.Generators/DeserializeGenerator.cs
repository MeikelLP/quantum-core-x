﻿using System.Text;
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
        var source = new StringBuilder();
        source.AppendLine($"        public void Deserialize(ReadOnlySpan<byte> bytes, in int offset = 0)");
        source.AppendLine("        {");
        source.AppendLine(GenerateMethodBody(type, dynamicByteIndex, "            ", false));
        source.AppendLine("        }");
        source.AppendLine();
        source.AppendLine(
            "        public async ValueTask DeserializeFromStreamAsync(Stream stream)");
        source.AppendLine("        {");
        source.AppendLine("            var buffer = ArrayPool<byte>.Shared.Rent(NetworkingConstants.BufferSize);");
        source.AppendLine("            try");
        source.AppendLine("            {");
        source.AppendLine(GenerateMethodBody(type, "", "                ", true));
        source.AppendLine("            }");
        source.AppendLine("            catch (Exception)");
        source.AppendLine("            {");
        source.AppendLine("                throw;");
        source.AppendLine("            }");
        source.AppendLine("            finally");
        source.AppendLine("            {");
        source.AppendLine("                ArrayPool<byte>.Shared.Return(buffer);");
        source.AppendLine("            }");
        source.AppendLine("        }");

        return source.ToString();
    }

    private string GenerateMethodBody(TypeDeclarationSyntax type, string dynamicByteIndex, string indentPrefix,
        bool isStreamMode)
    {
        var staticByteIndex = 0;
        var dynamicByteIndexLocal = new StringBuilder(dynamicByteIndex);
        var fields = _context.GetFieldsOfType(type);
        var typeStaticSize = fields.Sum(x => x.ElementSize);
        var fieldsCopy = fields.ToArray();
        var sb = new StringBuilder();
        // declare and initialize variables
        foreach (var field in fields)
        {
            var line = GetMethodLine(field, ref staticByteIndex, dynamicByteIndexLocal, "", indentPrefix, true,
                isStreamMode);

            // packets dynamic strings will send their size in an early field but this field is the size of the whole
            // packet not just the dynamic field's size
            var isDynamicSizeField = fieldsCopy.Any(x => x.SizeFieldName == field.Name);
            // + 1 because string includes a 0 byte at the end
            var staticSizeString = isDynamicSizeField ? $" - {typeStaticSize + 1}" : "";
            if (field.IsReadonly)
            {
                sb.Append($"{indentPrefix}var {GetVariableNameForExpression(field.Name)} = {line}{staticSizeString}");
            }
            else
            {
                sb.Append($"{indentPrefix}{field.Name} = {line}{staticSizeString}");
            }

            if (field is not {IsArray: true, HasDynamicLength: true} || (field.IsArray &&
                                                                         (field.SemanticType as IArrayTypeSymbol)
                                                                         ?.ElementType.Name == "Byte"))
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
            ? ((IArrayTypeSymbol)field.SemanticType).ElementType.Name
            : field.SemanticType.Name;
        if (field.IsEnum)
        {
            var enumType = field.SemanticType.GetFullName();

            return $"({enumType}) await stream.ReadEnumFromStreamAsync<{enumType}>(buffer)";
        }

        if (field.IsArray && ((IArrayTypeSymbol)field.SemanticType).ElementType.Name == "Byte")
        {
            // special handling for byte arrays
            // other array values are returned as usual when in stream mode

            if (field.HasDynamicLength)
            {
                return $"await stream.ReadByteArrayFromStreamAsync(buffer, {GetVariableNameForExpression(field.SizeFieldName!)})";
            }
            else
            {
                return $"await stream.ReadByteArrayFromStreamAsync(buffer, {field.ArrayLength})";
            }
        }

        if (typeName is "String")
        {
            var size = field.HasDynamicLength
                ? GetVariableNameForExpression(field.SizeFieldName!)
                : field.ElementSize.ToString();
            return $"await stream.ReadStringFromStreamAsync(buffer, (int){size})";
        }

        if (typeName is
            nameof(Byte) or
            nameof(SByte) or
            "Half" or // not in netstandard but net5.0 or newer
            nameof(UInt16) or
            nameof(Int16) or
            nameof(Single) or
            nameof(UInt32) or
            nameof(Int32) or
            nameof(Int64) or
            nameof(UInt64) or
            nameof(Double))
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
            {SemanticType.Name: "String"} => GetValueForString(field, ref offset, dynamicOffset, tempDynamicOffset,
                isStreamMode),
            // handle enum
            {IsEnum: true} => GetValueForSingleValue(field, (INamedTypeSymbol) field.SemanticType, ref offset,
                dynamicOffset, tempDynamicOffset, isStreamMode),
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
        return $"(bytes[{offsetStr}..{endOffsetStr}]).ReadNullTerminatedString()";
    }

    internal static string GetOffsetString(in int offset, StringBuilder dynamicOffset, string tempDynamicOffset,
        int? arrayLength = null, bool doNotPrependOffsetVariable = false)
    {
        var prefix = doNotPrependOffsetVariable
            ? ""
            : "offset + ";

        if ((prefix is "" && !doNotPrependOffsetVariable || prefix is not "" && doNotPrependOffsetVariable) &&
            dynamicOffset.Length == 0 && tempDynamicOffset is "")
        {
            return offset.ToString();
        }


        if (arrayLength.HasValue)
        {
            return
                $"(System.Index)({prefix}{offset}{dynamicOffset}{tempDynamicOffset})..(System.Index)({prefix}{offset}{dynamicOffset}{tempDynamicOffset} + {arrayLength})";
        }
        else
        {
            return $"(System.Index)({prefix}{offset}{dynamicOffset}{tempDynamicOffset})";
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

    internal static string GetValueForSingleValue(FieldData field, INamedTypeSymbol namedTypeSymbol, ref int offset,
        StringBuilder dynamicOffset, string tempDynamicOffset, bool isStreamMode)
    {
        if (isStreamMode)
        {
            return GetStreamReaderLine(field);
        }

        var cast = namedTypeSymbol.GetFullName();
        var typeName = namedTypeSymbol.TypeKind is TypeKind.Enum
            ? namedTypeSymbol.EnumUnderlyingType!.Name
            : namedTypeSymbol.Name;
        int? elementSize = GeneratorConstants.SupportedTypesByBitConverter.Contains(typeName)
            ? GeneratorConstants.GetSizeOfPrimitiveType(typeName)
            : null;
        var offsetStr = GetOffsetString(offset, dynamicOffset, tempDynamicOffset, elementSize);
        if (namedTypeSymbol.TypeKind is TypeKind.Enum)
        {
            var enumUnderlyingTypeName = namedTypeSymbol.EnumUnderlyingType!.Name;

            if (GeneratorConstants.SupportedTypesByBitConverter.Contains(enumUnderlyingTypeName))
            {
                return $"({cast})System.BitConverter.To{typeName}(bytes[{offsetStr}])";
            }

            if (GeneratorConstants.ConvertTypes.Contains(enumUnderlyingTypeName))
            {
                return $"({cast})System.Convert.To{typeName}(bytes[{offsetStr}])";
            }

            if (GeneratorConstants.NoCastTypes.Contains(enumUnderlyingTypeName))
            {
                return $"({cast})bytes[{offsetStr}]";
            }
        }

        if (GeneratorConstants.SupportedTypesByBitConverter.Contains(namedTypeSymbol.Name))
        {
            return $"System.BitConverter.To{typeName}(bytes[{offsetStr}])";
        }

        if (GeneratorConstants.NoCastTypes.Contains(namedTypeSymbol.Name))
        {
            return $"bytes[{offsetStr}]";
        }

        if (GeneratorConstants.ConvertTypes.Contains(namedTypeSymbol.Name))
        {
            return $"System.Convert.To{typeName}(bytes[{offsetStr}])";
        }

        if (namedTypeSymbol.Name == "String")
        {
            return GetValueForString(field, ref offset, dynamicOffset, tempDynamicOffset, isStreamMode);
        }

        throw new InvalidOperationException($"Don't know how to handle type {namedTypeSymbol.Name}");
    }

    private string GetLineForMisc(FieldData field, ref int offset, StringBuilder dynamicOffset,
        string tempDynamicOffset, string indentPrefix, bool isVariableMode, bool isStreamMode)
    {
        if (field.IsCustom)
        {
            // handle custom type
            return GetLineForInitializer(field.SemanticType, ref offset, dynamicOffset, tempDynamicOffset,
                indentPrefix, isVariableMode, isStreamMode);
        }
        else if (field.SemanticType is INamedTypeSymbol namedTypeSymbol)
        {
            return GetValueForSingleValue(field, namedTypeSymbol, ref offset, dynamicOffset, tempDynamicOffset,
                isStreamMode);
        }

        throw new ArgumentException($"Type of field is unknown: {field.SemanticType.GetFullName()}", nameof(field));
    }

    private string GetLineForArray(FieldData field, ref int offset, StringBuilder dynamicOffset,
        string tempDynamicOffset, string indentPrefix, bool isVariableMode, bool isStreamMode)
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
            return GetLineForFixedArray(field, ref offset, dynamicOffset, tempDynamicOffset, arr, indentPrefix,
                isVariableMode, isStreamMode);
        }
        else
        {
            return GetLineForDynamicArray(field, ref offset, dynamicOffset, tempDynamicOffset, arr, indentPrefix,
                isStreamMode);
        }
    }

    private string GetLineForDynamicArray(FieldData field, ref int offset,
        StringBuilder dynamicOffset, string tempDynamicOffset, IArrayTypeSymbol arr, string indent, bool isStreamMode)
    {
        var sb = new StringBuilder();
        if (arr.ElementType.Name == "Byte")
        {
            sb.Append(GetLineForDynamicByteArray(field, ref offset, dynamicOffset, tempDynamicOffset, isStreamMode));
        }
        else
        {
            sb.AppendLine(
                $"new {arr.ElementType.GetFullName()}[{GetVariableNameForExpression(field.SizeFieldName!)}];");
            sb.AppendLine($"{indent}for (var i = 0; i < {GetVariableNameForExpression(field.SizeFieldName!)}; i++)");
            sb.AppendLine($"{indent}{{");
            var variableName = field.Name;
            if (GeneratorContext.IsCustomType(arr.ElementType))
            {
                var type = _context.GetTypeDeclaration(arr.ElementType);
                var subFields = _context.GetFieldsOfType(type);
                foreach (var subField in subFields)
                {
                    var line = GetMethodLine(subField, ref offset, dynamicOffset,
                        $"{tempDynamicOffset} + {field.ElementSize} * i", indent, true, isStreamMode);
                    sb.AppendLine($"{indent}    {variableName}[i].{subField.Name} = {line};");
                }
            }
            else
            {
                var line = GetValueForSingleValue(field, (INamedTypeSymbol) arr.ElementType, ref offset, dynamicOffset,
                    $"{tempDynamicOffset} + {field.ElementSize} * i", isStreamMode);
                sb.AppendLine($"{indent}    {variableName}[i] = {line};");
            }

            sb.Append($"{indent}}}");
        }

        dynamicOffset.Append($" + {GetVariableNameForExpression(field.SizeFieldName!)}");

        return sb.ToString();
    }

    private string GetLineForFixedArray(FieldData field, ref int offset,
        StringBuilder dynamicOffset, string tempDynamicOffset, IArrayTypeSymbol arr, string indentPrefix,
        bool isVariableMode, bool isStreamMode)
    {
        var subTypeFullName = arr.ElementType.GetFullName()!;

        if (subTypeFullName == "System.Byte")
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
                if (GeneratorContext.IsCustomType(arr.ElementType))
                {
                    var initializer = GetLineForInitializer(arr.ElementType, ref offset, dynamicOffset,
                        tempDynamicOffset, $"    {indentPrefix}", isVariableMode, isStreamMode);
                    sb.Append($"{indentPrefix}    {initializer}");
                }
                else
                {
                    var elementType = (INamedTypeSymbol) ((IArrayTypeSymbol) field.SemanticType).ElementType;
                    var line = GetValueForSingleValue(field, elementType, ref offset, dynamicOffset, tempDynamicOffset,
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

    private string GetLineForInitializer(ITypeSymbol t, ref int offset, StringBuilder dynamicOffset,
        string tempDynamicOffset, string indentPrefix, bool isVariableMode, bool isStreamMode)
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
            sb.AppendLine();
            sb.AppendLine($"{indentPrefix}(");
            for (var ii = 0; ii < recordParamMembers.Length; ii++)
            {
                var member = recordParamMembers[ii];
                var valueStr = isVariableMode
                    ? GetMethodLine(member, ref offset, dynamicOffset, tempDynamicOffset, indentPrefix, isVariableMode,
                        isStreamMode)
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
                    ? GetMethodLine(member, ref offset, dynamicOffset, tempDynamicOffset, indentPrefix, isVariableMode,
                        isStreamMode)
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
}
