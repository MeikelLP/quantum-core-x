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

        var classWithAttributes = context.Compilation.SyntaxTrees
            .Where(st => st
                .GetRoot()
                .DescendantNodes()
                .OfType<TypeDeclarationSyntax>()
                .Any(p => p
                    .DescendantNodes()
                    .OfType<AttributeSyntax>()
                    .Any()));
        foreach (var tree in classWithAttributes)
        {
            var semanticModel = context.Compilation.GetSemanticModel(tree);
            var declaredClass = tree
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
            foreach (var type in declaredClass)
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
                var fullSize = fields.Sum(x => GetSize(x.Type)) + 1;
                var header = attr.ArgumentList!.Arguments[0].ToString();
                var typeKeyWords = GetTypeKeyWords(type);

                var source = new StringBuilder();
                ApplyHeader(source, typeKeyWords, ns, name);
                source.AppendLine(GenerateWriteHeader(header));
                var byteIndex = 1;
                foreach (var field in fields)
                {
                    var fieldSize = GetSize(field.Type);
                    source.AppendLine(GenerateWriteField(field.Name, fieldSize, byteIndex, field.Type == "bool"));
                    byteIndex += fieldSize;
                }

                ApplyFooter(source, fullSize);

                context.AddSource($"{name}.g.cs", SourceText.From(source.ToString(), Encoding.UTF8));
            }
        }
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

    private static string GenerateWriteField(string name, int size, int index, bool isBool = false)
    {
        if (size == 1)
        {
            var byteCast = isBool ? "(byte)" : "";
            return $"            bytes[offset + {index}] = {byteCast}this.{name};";
        }

        return $"            System.BitConverter.GetBytes(this.{name}).CopyTo(bytes, offset + {index});";
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

    private static void ApplyFooter(StringBuilder sb, int size)
    {
        sb.Append(
            $@"        }}

        public ushort GetSize() {{
            return {size};
        }}
    }}
}}".Trim('\n'));
    }

    private static int GetSize(string fieldType)
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
            default:
                throw new NotImplementedException("Don't know how to handle Enum, Array or string");
        }
    }
}