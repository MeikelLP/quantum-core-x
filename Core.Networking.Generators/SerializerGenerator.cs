using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace QuantumCore.Networking;

[Generator]
public class SerializerGenerator : ISourceGenerator
{
    private INamedTypeSymbol _packetFieldAttributeType = null!;
    private INamedTypeSymbol _packetAttributeType = null!;
    private INamedTypeSymbol _generatorAttributeType = null!;
    private IDictionary<string, (TypeDeclarationSyntax TypeDeclaration, bool GenerateFor)> _relevantTypes = null!;
    private IEnumerable<SemanticModel> _semanticModels = null!;

    private static string[] _supportedTypesByBitConverter =
        { "Half", "Double", "Single", "Int16", "Int32", "Int64", "UInt16", "UInt32", "UInt64", "Char" };

    private static string[] _noCastTypes = { "Byte", "SByte" };
    private static string[] _castableToByteTypes = { "Boolean" };

    public void Execute(GeneratorExecutionContext context)
    {
        _generatorAttributeType =
            context.Compilation.GetTypeByMetadataName("QuantumCore.Networking.PacketGeneratorAttribute")!
                .OriginalDefinition;
        _packetAttributeType =
            context.Compilation.GetTypeByMetadataName("QuantumCore.Core.Networking.PacketAttribute")!
                .OriginalDefinition;
        _packetFieldAttributeType =
            context.Compilation.GetTypeByMetadataName("QuantumCore.Core.Networking.FieldAttribute")!
                .OriginalDefinition;

        _semanticModels = context.Compilation.SyntaxTrees.Select(x => context.Compilation.GetSemanticModel(x));
        _relevantTypes = GetRelevantTypes(context.Compilation.SyntaxTrees);
        var typesToGenerateFor = _relevantTypes.Where(x => x.Value.GenerateFor).ToArray();
        foreach (var pair in typesToGenerateFor)
        {
            var (name, source) = GenerateFile(pair.Value.TypeDeclaration, pair.Value.TypeDeclaration.SyntaxTree);

            context.AddSource($"{name}.g.cs", SourceText.From(source, Encoding.UTF8));
        }
    }

    private ITypeSymbol? GetTypeInfo(BaseTypeDeclarationSyntax type)
    {
        return _semanticModels.Select(x => x.GetDeclaredSymbol(type)).FirstOrDefault();
    }

    private ITypeSymbol? GetTypeInfo(SyntaxNode type)
    {
        return _semanticModels.FirstOrDefault(x => x.GetTypeInfo(type).Type != null)?.GetTypeInfo(type).Type;
    }

    private IDictionary<string, (TypeDeclarationSyntax TypeDeclaration, bool GenerateFor)> GetRelevantTypes(
        IEnumerable<SyntaxTree> syntaxTrees)
    {
        var allTypeDeclarations = syntaxTrees
            .SelectMany(x => x.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
            .ToArray();
        var typesToGenerateFor = allTypeDeclarations
            .Where(x => x
                .AttributeLists
                .SelectMany(list => list.Attributes)
                .Any(attr => SymbolEqualityComparer.Default.Equals(GetTypeInfo(attr), _generatorAttributeType))
            )
            .ToDictionary(x => GetTypeInfo(x)!.GetFullName(), x => (x, true));
        var noGeneratorButRelevantTypes = new Dictionary<string, (TypeDeclarationSyntax, bool)>();
        foreach (var keyPair in typesToGenerateFor)
        {
            var fields = GetMemberDefinitions(keyPair.Value.x);
            var includedCustomTypes = fields
                .Select(x =>
                {
                    var typeInfo = GetTypeInfo(x);
                    if (typeInfo is IArrayTypeSymbol arr && IsCustomType(arr.ElementType))
                    {
                        return arr.ElementType;
                    }

                    return typeInfo;
                })
                .Where(IsCustomType!)
                .ToArray();
            foreach (var includedCustomType in includedCustomTypes)
            {
                var customType = allTypeDeclarations.FirstOrDefault(x =>
                                     SymbolEqualityComparer.Default.Equals(GetTypeInfo(x), includedCustomType))
                                 ?? throw new InvalidOperationException(
                                     "Type cannot be used as it is not defined in the same assembly as packet type");
                noGeneratorButRelevantTypes.Add(GetTypeInfo(customType)!.GetFullName(), (customType, false));
            }
        }

        return typesToGenerateFor
            .Concat(noGeneratorButRelevantTypes)
            .ToDictionary(x => x.Key, x => x.Value);
    }

    private IEnumerable<TypeSyntax> GetMemberDefinitions(TypeDeclarationSyntax type)
    {
        var fields = new List<TypeSyntax>();
        if (type is RecordDeclarationSyntax record)
        {
            fields.AddRange(record.ParameterList!.Parameters.Select(p => p.Type!));
        }

        fields.AddRange(type
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .Select(x => x.Type));
        fields.AddRange(type
            .DescendantNodes()
            .OfType<FieldDeclarationSyntax>()
            .Select(x => x.Declaration.Type));

        return fields;
    }

    private (string Name, string Source) GenerateFile(TypeDeclarationSyntax type, SyntaxTree tree)
    {
        var attr = type
            .DescendantNodes()
            .OfType<AttributeSyntax>()
            .FirstOrDefault(a => a
                .DescendantTokens()
                .Any(dt => dt.IsKind(SyntaxKind.IdentifierToken) &&
                           SymbolEqualityComparer.Default.Equals(GetTypeInfo(dt.Parent!),
                               _packetAttributeType)));

        if (attr is null)
        {
            throw new InvalidOperationException(
                "PacketGeneratorAttribute requires PacketAttribute to be set as well");
        }

        var name = type.Identifier.Text;
        var ns = tree.GetRoot().DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>().First()?.Name.ToString()!;
        var fields = GetFieldsOfType(type);
        var staticSize = GetStaticSizeOfType(fields) + 1; // + header size
        var dynamicSize = string.Join(" + ",
            fields.Where(x => x.HasDynamicLength).Select(x =>
            {
                var multiplier = x.IsArray && x.ElementSize > 1
                    ? $" * {x.ElementSize}" 
                    : "";
                return $"this.{x.Name}.Length{multiplier}";
            }));
        var header = attr.ArgumentList!.Arguments[0].ToString();
        var typeKeyWords = GetTypeKeyWords(type);

        var source = new StringBuilder();
        ApplyHeader(source, typeKeyWords, ns, name);
        source.AppendLine(GenerateWriteHeader(header));
        var staticByteIndex = 1;
        var dynamicByteIndex = new StringBuilder();
        foreach (var field in fields)
        {
            var line = GenerateMethodLine(field, $"this.{field.Name}", ref staticByteIndex, dynamicByteIndex, "", "            ");
            source.AppendLine(line);
        }

        ApplyFooter(source, staticSize, dynamicSize);
        return (name, source.ToString());
    }

    private static int GetStaticSizeOfType(IReadOnlyList<FieldData> fields)
    {
        return fields.Sum(x => x.FieldSize);
    }

    private static string GetTypeKeyWords(TypeDeclarationSyntax type)
    {
        var typeKeyWords = type switch
        {
            StructDeclarationSyntax => "struct",
            RecordDeclarationSyntax { ClassOrStructKeyword.Text: "struct" } => "record struct",
            RecordDeclarationSyntax => "record",
            _ => "class"
        };

        return typeKeyWords;
    }

    private IReadOnlyList<FieldData> GetFieldsOfType(TypeDeclarationSyntax type)
    {
        var fields = new List<FieldData>();
        if (type is RecordDeclarationSyntax record)
        {
            fields.AddRange(record.ParameterList!.Parameters
                .Select(x => BuildFieldData(x.Type!, x.Identifier))
            );
        }

        fields.AddRange(type.Members
            .OfType<PropertyDeclarationSyntax>()
            .Select(x =>
            {
                var orderStr = x.AttributeLists.SelectMany(attr => attr.Attributes).FirstOrDefault(attr =>
                    SymbolEqualityComparer.Default.Equals(GetTypeInfo(attr),
                        _packetFieldAttributeType))?.ArgumentList!.Arguments[0].Expression.ToString();
                var arrayLength = GetArrayLength(x);
                return BuildFieldData(x.Type, x.Identifier, arrayLength, orderStr);
            })
        );
        var finalArr = new List<FieldData>(fields.Count);
        // first add all fields normally
        finalArr.AddRange(fields.Where(x => !x.Order.HasValue));
        // then insert overriden fields to their desired position
        foreach (var field in fields.Where(x => x.Order.HasValue).OrderBy(x => x.Order))
        {
            finalArr.Insert(field.Order!.Value, field);
        }

        return finalArr;
    }

    private FieldData BuildFieldData(TypeSyntax type, SyntaxToken name, int? arrayLength = null,
        string? orderStr = null)
    {
        var isArray = type is ArrayTypeSyntax;
        var fieldType = GetTypeInfo(type)!;
        var enumType = isArray ? null : (INamedTypeSymbol)fieldType;
        var isEnum = enumType?.TypeKind is TypeKind.Enum;
        return new FieldData
        {
            Name = name.Text,
            SemanticType = fieldType,
            IsArray = isArray,
            IsEnum = isEnum,
            IsCustom = IsCustomType(fieldType),
            ArrayLength = arrayLength,
            ElementSize = GetStaticSize(fieldType, arrayLength),
            Order = orderStr != null ? int.Parse(orderStr) : null
        };
    }

    private static bool IsCustomType(ITypeSymbol fieldType)
    {
        return !fieldType.GetFullName()!.StartsWith("System.") &&
               fieldType.TypeKind is not TypeKind.Enum and not TypeKind.Array;
    }

    private string GetTypeFromProperty(TypeSyntax x)
    {
        return x switch
        {
            ArrayTypeSyntax arrayTypeSyntax => ((PredefinedTypeSyntax)arrayTypeSyntax.ElementType).Keyword.Text,
            PredefinedTypeSyntax predefinedTypeSyntax => predefinedTypeSyntax.Keyword.Text,
            IdentifierNameSyntax identifierNameSyntax =>
                ((INamedTypeSymbol)GetTypeInfo(identifierNameSyntax)!).TypeKind is TypeKind.Enum
                    ? ExplicitPrimitiveTypeNameToSimple(
                        ((INamedTypeSymbol)GetTypeInfo(identifierNameSyntax)!).EnumUnderlyingType!.Name)
                    : $"$custom->{((INamedTypeSymbol)GetTypeInfo(identifierNameSyntax)!).Name}",
            _ => throw new InvalidOperationException($"Don't know how to handle syntax node {x}")
        };
    }

    private static string ExplicitPrimitiveTypeNameToSimple(string explicitPrimitiveTypeName)
    {
        return explicitPrimitiveTypeName switch
        {
            "Int32" => "int",
            "Single" => "float",
            "Double" => "double",
            "Int64" => "long",
            "Byte" => "byte",
            _ => throw new ArgumentOutOfRangeException(nameof(explicitPrimitiveTypeName))
        };
    }

    private static int? GetArrayLength(PropertyDeclarationSyntax x)
    {
        if (x is
            {
                Type: ArrayTypeSyntax, Initializer: not null, Initializer.Value: ArrayCreationExpressionSyntax
                {
                    Type.RankSpecifiers.Count: 1
                } arrayCreationExpressionSyntax
            } && arrayCreationExpressionSyntax.Type.RankSpecifiers[0].Sizes.OfType<LiteralExpressionSyntax>().Any())
        {
            return (int?)arrayCreationExpressionSyntax.Type.RankSpecifiers[0].Sizes.OfType<LiteralExpressionSyntax>()
                .First().Token.Value;
        }

        return null;
    }

    public void Initialize(GeneratorInitializationContext context)
    {
    }

    private static string GenerateWriteHeader(string header)
    {
        return $"            bytes[offset + 0] = {header};";
    }

    private string GenerateMethodLine(FieldData field, string fieldExpression, ref int offset, StringBuilder dynamicOffset, string tempDynamicOffset, string indentPrefix)
    {
        var finalLine = field switch
        {
            // handle byte[]
            { IsArray: true, SemanticType: IArrayTypeSymbol { ElementType.Name: "Byte" } } =>
                $"{indentPrefix}{fieldExpression}.CopyTo(bytes, offset + {offset}{dynamicOffset});",
            // handle Custom[]
            { IsArray: true } => GetLineForArray(field, fieldExpression, ref offset, dynamicOffset, tempDynamicOffset, indentPrefix),
            // handle string
            { SemanticType.Name: "String" } => GetLineForString(fieldExpression, ref offset, dynamicOffset, tempDynamicOffset, indentPrefix),
            // handle enum
            { IsEnum: true } => GetLineForSingleValue((INamedTypeSymbol)field.SemanticType, fieldExpression, ref offset, dynamicOffset, tempDynamicOffset, indentPrefix),
            // misc
            _ => GenerateLineForMisc(field, fieldExpression, ref offset, dynamicOffset, tempDynamicOffset, indentPrefix)
        };


        var fieldSize = field.ArrayLength ?? 1 * field.ElementSize;
        offset += fieldSize;

        // handle anything else
        return finalLine;
    }

    private static string GetLineForString(string fieldExpression, ref int offset, StringBuilder dynamicOffset,
        string tempDynamicOffset, string indentPrefix)
    {
        var offsetStr = $"offset + {offset}{dynamicOffset}{tempDynamicOffset}";
        dynamicOffset.Append($" + {fieldExpression}.Length");
        return $"{indentPrefix}System.Text.Encoding.ASCII.GetBytes({fieldExpression}).CopyTo(bytes, {offsetStr});";
    }

    private static string GetLineForSingleValue(INamedTypeSymbol namedTypeSymbol, string fieldExpression, ref int offset,
        StringBuilder dynamicOffset, string tempDynamicOffset, string indentPrefix)
    {
        var cast = namedTypeSymbol.GetFullName();

        var offsetStr = $"offset + {offset}{dynamicOffset}{tempDynamicOffset}";
        if (namedTypeSymbol.TypeKind is TypeKind.Enum)
        {
            var enumUnderlyingTypeName = namedTypeSymbol.EnumUnderlyingType!.Name;
            var enumCast = namedTypeSymbol.EnumUnderlyingType.GetFullName();

            if (_supportedTypesByBitConverter.Contains(enumUnderlyingTypeName))
            {
                return $"{indentPrefix}System.BitConverter.GetBytes(({enumCast}){fieldExpression}).CopyTo(bytes, {offsetStr});";
            }
            if (_castableToByteTypes.Contains(enumUnderlyingTypeName))
            {
                return $"{indentPrefix}bytes[{offsetStr}] = ({enumCast}){fieldExpression};";
            }

            if (_noCastTypes.Contains(enumUnderlyingTypeName))
            {
                return $"{indentPrefix}bytes[{offsetStr}] = (byte){fieldExpression};";
            }
        }

        if (_supportedTypesByBitConverter.Contains(namedTypeSymbol.Name))
        {
            return $"{indentPrefix}System.BitConverter.GetBytes({fieldExpression}).CopyTo(bytes, {offsetStr});";
        }

        if (_noCastTypes.Contains(namedTypeSymbol.Name))
        {
            return $"{indentPrefix}bytes[{offsetStr}] = {fieldExpression};";
        }

        if (_castableToByteTypes.Contains(namedTypeSymbol.Name))
        {
            return $"{indentPrefix}bytes[{offsetStr}] = ({cast}){fieldExpression};";
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
            if (!_relevantTypes.TryGetValue(fieldTypeFullName!, out var type))
            {
                throw new InvalidOperationException(
                    $"Could not find type declaration for type {fieldTypeFullName}");
            }

            var subFields = GetFieldsOfType(type.TypeDeclaration);
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
            return GetLineForSingleValue(namedTypeSymbol, fieldExpression, ref offset, dynamicOffset, tempDynamicOffset, indentPrefix);
        }

        throw new NotImplementedException("???");
    }

    private string GetLineForArray(FieldData field, string fieldExpression, ref int offset, StringBuilder dynamicOffset,
        string tempDynamicOffset, string indentPrefix)
    {
        if (field.SemanticType is IArrayTypeSymbol arr && IsCustomType(arr.ElementType))
        {
            if (field.ArrayLength.HasValue)
            {
                var subTypeFullName = arr.ElementType.GetFullName()!;
                if (!_relevantTypes.TryGetValue(subTypeFullName, out var subType))
                {
                    throw new InvalidOperationException($"Could not find required type {subTypeFullName}");
                }

                // iterate over each item in array
                var lines = new List<string>();
                for (int i = 0; i < field.ArrayLength.Value; i++)
                {
                    // recursive call to generate lines for each field in sub type
                    var type = GetFieldsOfType(subType.TypeDeclaration);
                    for (var ii = 0; ii < type.Count; ii++)
                    {
                        var member = type[ii];
                        var line = GenerateMethodLine(member, $"{fieldExpression}[{i}].{member.Name}", ref offset, dynamicOffset, tempDynamicOffset, indentPrefix);
                        lines.Add(line);
                    }
                }

                return string.Join("\r\n", lines);
            }
            else
            {
                var subTypeFullName = arr.ElementType.GetFullName()!;
                if (!_relevantTypes.TryGetValue(subTypeFullName, out var subType))
                {
                    throw new InvalidOperationException($"Could not find required type {subTypeFullName}");
                }
                
                // iterate over each item in array
                var lines = new List<string>
                {
                    $"{indentPrefix}for (var i = 0; i < {fieldExpression}.Length; i++)",
                    $"{indentPrefix}{{"
                };
                // recursive call to generate lines for each field in sub type
                var type = GetFieldsOfType(subType.TypeDeclaration);

                for (var ii = 0; ii < type.Count; ii++)
                {
                    var member = type[ii];
                    var subFieldExpression = $"{fieldExpression}[i].{member.Name}";
                    var line = GenerateMethodLine(member, subFieldExpression, ref offset, dynamicOffset, $" + i * {member.ElementSize}", $"{indentPrefix}    ");
                    lines.Add(line);
                }
                lines.Add($"{indentPrefix}}}");

                return string.Join("\r\n", lines);
            }
        }

        throw new NotImplementedException(
            $"Don't know how to handle array of {((IArrayTypeSymbol)field.SemanticType).ElementType}");
    }

    private static void ApplyHeader(StringBuilder sb, string typeKeywords, string ns, string name)
    {
        sb.AppendLine($@"/// <auto-generated/>
using QuantumCore.Networking;

namespace {ns} {{

    public partial {typeKeywords} {name} : IPacketSerializable
    {{
        public void Serialize(byte[] bytes, int offset = 0) {{");
    }

    private static void ApplyFooter(StringBuilder sb, int size, string dynamicSize)
    {
        sb.Append(@"        }

        public ushort GetSize() {
            return ");

        sb.Append(!string.IsNullOrWhiteSpace(dynamicSize)
            ? $"(ushort)({size} + {(!string.IsNullOrWhiteSpace(dynamicSize) ? dynamicSize : "")})"
            : size.ToString());

        sb.Append(@";
        }
    }
}".Trim('\n'));
    }

    private int GetStaticSize(ITypeSymbol semanticType, int? arrayLength = null)
    {
        var typeName = semanticType.Name;

        switch (typeName)
        {
            case "Int32":
            case "UInt32":
            case "Single":
                return 4;
            case "Byte":
            case "SByte":
            case "Boolean":
                return 1;
            case "Int16":
            case "UInt16":
                return 2;
            case "String":
                // dynamic size - does not contribute to static size
                return 0;
            default:
                if (IsCustomType(semanticType))
                {
                    // probably a custom type
                    if (!_relevantTypes.TryGetValue(semanticType.GetFullName()!, out var customType))
                    {
                        throw new InvalidOperationException(
                            $"Could not find syntax tree for custom type {semanticType.GetFullName()}");
                    }

                    var fields = GetFieldsOfType(customType.TypeDeclaration);
                    return GetStaticSizeOfType(fields);
                }
                else if (semanticType is IArrayTypeSymbol arr)
                {
                    if (arrayLength is null)
                    {
                        // may have dynamic size
                        return GetStaticSize(arr.ElementType);
                    }
                    else
                    {
                        return GetStaticSize(arr.ElementType);
                    }
                }
                else if (semanticType.TypeKind is TypeKind.Enum && semanticType is INamedTypeSymbol namedTypeSymbol)
                {
                    return GetStaticSize(namedTypeSymbol.EnumUnderlyingType!);
                }

                throw new NotImplementedException($"Don't know how to handle {semanticType.Name}");
        }
    }
}