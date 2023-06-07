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
        var staticSize = fields.Sum(x => GetStaticSize(x.Type)) + 1;
        var dynamicSize = string.Join(" + ",
            fields.Where(x => x.Type == "string").Select(x => $"{x.Name}.Length"));
        var header = attr.ArgumentList!.Arguments[0].ToString();
        var typeKeyWords = GetTypeKeyWords(type);

        var source = new StringBuilder();
        ApplyHeader(source, typeKeyWords, ns, name);
        source.AppendLine(GenerateWriteHeader(header));
        var staticByteIndex = 1;
        var dynamicByteIndex = "";
        foreach (var field in fields)
        {
            var fieldSize = GetStaticSize(field.Type);
            source.AppendLine(GenerateWriteField(field.Name, field.Type, fieldSize, staticByteIndex, dynamicByteIndex));
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
        string typeKeyWords;
        if (type is StructDeclarationSyntax)
        {
            typeKeyWords = "struct";
        }
        else if (type is RecordDeclarationSyntax recordDeclarationSyntax)
        {
            typeKeyWords = recordDeclarationSyntax.ClassOrStructKeyword.Text == "struct"
                ? "record struct"
                : "record";
        }
        else
        {
            typeKeyWords = "class";
        }

        return typeKeyWords;
    }

    private IReadOnlyList<(string Name, string Type, int? Order)> GetFieldsOfType(SemanticModel semanticModel,
        INamedTypeSymbol packetFieldAttributeType,
        TypeDeclarationSyntax type)
    {
        // use ridiculously high number to start counting forward to not collide with user defined values 
        var order = int.MaxValue / 2;
        var fields = new List<(string Name, string Type, int? Order)>();
        if (type is RecordDeclarationSyntax record)
        {
            fields.AddRange(record.ParameterList!.Parameters
                .Select(x => (
                        x.Identifier.Text,
                        x.Type is PredefinedTypeSyntax predefinedTypeSyntax
                            ? predefinedTypeSyntax.Keyword.Text
                            : throw new InvalidOperationException(),
                        (int?)order++
                    )
                )
            );
        }

        fields.AddRange(type.Members
            .OfType<PropertyDeclarationSyntax>()
            .Select(x =>
            {
                var orderStr = x.AttributeLists.SelectMany(attr => attr.Attributes).FirstOrDefault(attr =>
                    SymbolEqualityComparer.Default.Equals(semanticModel.GetTypeInfo(attr).Type,
                        packetFieldAttributeType))?.ArgumentList!.Arguments[0].Expression.ToString();
                return (
                    x.Identifier.Text,
                    x.Type is PredefinedTypeSyntax predefinedTypeSyntax
                        ? predefinedTypeSyntax.Keyword.Text
                        : throw new InvalidOperationException(),
                    orderStr != null ? int.Parse(orderStr) : (int?)order++
                );
            })
        );
        return fields.OrderBy(x => x.Order).ToList();
    }

    public void Initialize(GeneratorInitializationContext context)
    {
    }

    private static string GenerateWriteHeader(string header)
    {
        return $"            bytes[0] = {header};";
    }

    private static string GenerateWriteField(string name, string type, int size, int offset, string dynamicOffset)
    {
        var offsetStr = $"offset + {offset}{dynamicOffset}";
        if (size == 1)
        {
            var byteCast = type == "bool" ? "(byte)" : "";
            return $"            bytes[{offsetStr}] = {byteCast}this.{name};";
        }
        else if (size == 0 && type == "string")
        {
            return $"            System.Text.Encoding.ASCII.GetBytes(this.{name}).CopyTo(bytes, {offsetStr});";
        }

        return $"            System.BitConverter.GetBytes(this.{name}).CopyTo(bytes, {offsetStr});";
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
        sb.Append(
            @"        }

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
        switch (fieldType)
        {
            case "int":
            case "uint":
            case "float":
                return 4;
            case "byte":
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
                throw new NotImplementedException("Don't know how to handle enum or array");
        }
    }
}