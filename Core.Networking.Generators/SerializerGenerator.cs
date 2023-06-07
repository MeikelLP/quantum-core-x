using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace QuantumCore.Networking;

[Generator]
public class SerializerGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        var generatorAttributeType =
            context.Compilation.GetTypeByMetadataName("QuantumCore.Networking.PacketGeneratorAttribute")!
                .OriginalDefinition;
        var packetAttributeType =
            context.Compilation.GetTypeByMetadataName("QuantumCore.Core.Networking.PacketAttribute")!
                .OriginalDefinition;
        var packetFieldAttributeType =
            context.Compilation.GetTypeByMetadataName("QuantumCore.Core.Networking.FieldAttribute")!
                .OriginalDefinition;

        var filesWithClasses = context.Compilation.SyntaxTrees
            .Where(st => st
                .GetRoot()
                .DescendantNodes()
                .OfType<TypeDeclarationSyntax>()
                .Any(p => p
                    .DescendantNodes()
                    .OfType<AttributeSyntax>()
                    .Any()));
        foreach (var file in filesWithClasses)
        {
            var semanticModel = context.Compilation.GetSemanticModel(file);
            var declaredTypes = file
                .GetRoot()
                .DescendantNodes()
                .OfType<TypeDeclarationSyntax>()
                .Where(cd => cd
                    .DescendantNodes()
                    .OfType<AttributeSyntax>()
                    .Any(attr =>
                        SymbolEqualityComparer.Default.Equals(semanticModel.GetTypeInfo(attr).Type,
                            generatorAttributeType))
                )
                .ToArray();
            foreach (var type in declaredTypes)
            {
                var (name, source) =
                    GenerateFile(type, semanticModel, packetAttributeType, file, packetFieldAttributeType);

                context.AddSource($"{name}.g.cs", SourceText.From(source, Encoding.UTF8));
            }
        }
    }

    private (string Name, string Source) GenerateFile(TypeDeclarationSyntax type, SemanticModel semanticModel,
        ISymbol packetAttributeType, SyntaxTree tree, INamedTypeSymbol packetFieldAttributeType)
    {
        var attr = type
            .DescendantNodes()
            .OfType<AttributeSyntax>()
            .FirstOrDefault(a => a
                .DescendantTokens()
                .Any(dt => dt.IsKind(SyntaxKind.IdentifierToken) &&
                           SymbolEqualityComparer.Default.Equals(semanticModel.GetTypeInfo(dt.Parent!).Type,
                               packetAttributeType)));

        if (attr is null)
        {
            throw new InvalidOperationException(
                "PacketGeneratorAttribute requires PacketAttribute to be set as well");
        }

        var name = type.Identifier.Text;
        var ns = tree.GetRoot().DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>().First()?.Name
            .ToString()!;
        var fields = GetFieldsOfType(semanticModel, packetFieldAttributeType, type);
        var staticSize = fields.Sum(x => x.FieldSize) + 1;
        var dynamicSize = string.Join(" + ",
            fields.Where(x => x.HasDynamicLength).Select(x => $"this.{x.Name}.Length"));
        var header = attr.ArgumentList!.Arguments[0].ToString();
        var typeKeyWords = GetTypeKeyWords(type);

        var source = new StringBuilder();
        ApplyHeader(source, typeKeyWords, ns, name);
        source.AppendLine(GenerateWriteHeader(header));
        var staticByteIndex = 1;
        var dynamicByteIndex = "";
        foreach (var field in fields)
        {
            var fieldSize = field.ArrayLength ?? 1 * field.ElementSize;
            source.AppendLine(GenerateWriteField(field, staticByteIndex, dynamicByteIndex));
            if (field.Type == "string")
            {
                dynamicByteIndex += $" + this.{field.Name}.Length";
            }

            staticByteIndex += fieldSize;
        }

        ApplyFooter(source, staticSize, dynamicSize);
        return (name, source.ToString());
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

    private IReadOnlyList<FieldData> GetFieldsOfType(
        SemanticModel semanticModel,
        ISymbol packetFieldAttributeType,
        TypeDeclarationSyntax type)
    {
        var fields = new List<FieldData>();
        if (type is RecordDeclarationSyntax record)
        {
            fields.AddRange(record.ParameterList!.Parameters
                .Select(x => BuildFieldData(semanticModel, x.Type!, x.Identifier))
            );
        }

        fields.AddRange(type.Members
            .OfType<PropertyDeclarationSyntax>()
            .Select(x =>
            {
                var orderStr = x.AttributeLists.SelectMany(attr => attr.Attributes).FirstOrDefault(attr =>
                    SymbolEqualityComparer.Default.Equals(semanticModel.GetTypeInfo(attr).Type,
                        packetFieldAttributeType))?.ArgumentList!.Arguments[0].Expression.ToString();
                var arrayLength = GetArrayLength(x);
                return BuildFieldData(semanticModel, x.Type, x.Identifier, arrayLength, orderStr);
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

    private static FieldData BuildFieldData(SemanticModel semanticModel, TypeSyntax type, SyntaxToken name, int? arrayLength = null, string? orderStr = null)
    {
        var fieldTypeName = GetTypeFromProperty(semanticModel, type);
        var isArray = type is ArrayTypeSyntax;
        var enumType = isArray ? null : (INamedTypeSymbol)semanticModel.GetTypeInfo(type).Type!;
        var isEnum = enumType?.TypeKind is TypeKind.Enum;
        return new FieldData
        {
            Name = name.Text,
            Type = fieldTypeName,
            IsArray = isArray,
            IsEnum = isEnum,
            ArrayLength = arrayLength,
            ElementSize = GetStaticSize(fieldTypeName),
            Order = orderStr != null ? int.Parse(orderStr) : null
        };
    }

    private static string GetTypeFromProperty(SemanticModel semanticModel, TypeSyntax x)
    {
        return x switch
        {
            ArrayTypeSyntax arrayTypeSyntax => ((PredefinedTypeSyntax)arrayTypeSyntax.ElementType).Keyword.Text,
            PredefinedTypeSyntax predefinedTypeSyntax => predefinedTypeSyntax.Keyword.Text,
            IdentifierNameSyntax identifierNameSyntax  => ((INamedTypeSymbol)semanticModel.GetTypeInfo(identifierNameSyntax).Type!).TypeKind is TypeKind.Enum
                ? ExplicitPrimitiveTypeNameToSimple(((INamedTypeSymbol)semanticModel.GetTypeInfo(identifierNameSyntax).Type!).EnumUnderlyingType!.Name)
                : throw new InvalidOperationException($"Don't know how to handle IdentifierNameSyntax {x}"),
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
            return (int?) arrayCreationExpressionSyntax.Type.RankSpecifiers[0].Sizes.OfType<LiteralExpressionSyntax>().First().Token.Value;
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

    private static string GenerateWriteField(FieldData field, int offset, string dynamicOffset)
    {
        var offsetStr = $"offset + {offset}{dynamicOffset}";
        const string prefix = "            ";
        switch (field)
        {
            case { IsArray: true, Type: "byte" }:
                return $"{prefix}this.{field.Name}.CopyTo(bytes, {offsetStr});";
            case { ElementSize: 0, Type: "string" }:
                return $"{prefix}System.Text.Encoding.ASCII.GetBytes(this.{field.Name}).CopyTo(bytes, {offsetStr});";
            case { IsEnum: true, Type: not "byte" }:
                return $"{prefix}System.BitConverter.GetBytes(({field.Type})this.{field.Name}).CopyTo(bytes, {offsetStr});";
            default:
            {
                if (field.ElementSize == 1)
                {
                    var byteCast = field.Type == "bool" || field.IsEnum ? "(byte)" : "";
                    return $"{prefix}bytes[{offsetStr}] = {byteCast}this.{field.Name};";
                }

                break;
            }
        }

        return $"{prefix}System.BitConverter.GetBytes(this.{field.Name}).CopyTo(bytes, {offsetStr});";
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

    private static int GetStaticSize(string fieldType)
    {
        if (fieldType.EndsWith("[]"))
        {
            // may have dynamic size
            return 0;
        }

        switch (fieldType)
        {
            case "int":
            case "Int32":
            case "uint":
            case "float":
                return 4;
            case "byte":
            case "Byte":
            case "sbyte":
            case "bool":
                return 1;
            case "short":
            case "ushort":
                return 2;
            case "string":
                // dynamic size - does not contribute to static size
                return 0;
            default:
                throw new NotImplementedException($"Don't know how to handle {fieldType}");
        }
    }
}