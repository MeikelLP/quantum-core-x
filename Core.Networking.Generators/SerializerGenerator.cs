﻿using System.Text;
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
        var generatorAttributeType = context.Compilation.GetTypeByMetadataName("QuantumCore.Networking.PacketGeneratorAttribute")!.OriginalDefinition;
        var packetAttributeType = context.Compilation.GetTypeByMetadataName("QuantumCore.Core.Networking.PacketAttribute")!.OriginalDefinition;

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
                    .Any(attr => SymbolEqualityComparer.Default.Equals(semanticModel.GetTypeInfo(attr).Type, generatorAttributeType))
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
                                            SymbolEqualityComparer.Default.Equals(semanticModel.GetTypeInfo(dt.Parent!).Type, packetAttributeType)));

                if (attr is null)
                {
                    throw new InvalidOperationException(
                        "PacketGeneratorAttribute requires PacketAttribute to be set as well");
                }
                var name = type.Identifier.Text;
                var ns = tree.GetRoot().DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>().First()?.Name.ToString()!;
                var fields = GetFieldsOfType(type);
                var fullSize = fields.Sum(x => GetSize(x.Type)) + 1;
                var header = attr.ArgumentList!.Arguments[0].ToString();
                var isStruct = type is StructDeclarationSyntax or RecordDeclarationSyntax { ClassOrStructKeyword.Text: "struct" }; 

                var source = new StringBuilder();
                ApplyHeader(source, type is RecordDeclarationSyntax, isStruct, ns, name);
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

    private IEnumerable<(string Name, string Type)> GetFieldsOfType(TypeDeclarationSyntax type)
    {
        if (type is RecordDeclarationSyntax record)
        {
            return record.ParameterList!.Parameters
                .Select(x => (x.Identifier.Text,
                    x.Type is PredefinedTypeSyntax predefinedTypeSyntax
                        ? predefinedTypeSyntax.Keyword.Text
                        : throw new InvalidOperationException()))
                .ToArray();
        }
        return type.Members
            .OfType<PropertyDeclarationSyntax>()
            .Select(x => (x.Identifier.Text,
                x.Type is PredefinedTypeSyntax predefinedTypeSyntax
                    ? predefinedTypeSyntax.Keyword.Text
                    : throw new InvalidOperationException()))
            .ToArray();
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

    private static void ApplyHeader(StringBuilder sb, bool isRecord, bool isStruct, string ns, string name)
    {
        sb.AppendLine($@"/// <auto-generated/>
using QuantumCore.Networking;

namespace {ns} {{

    public partial {(isRecord ? "record " : "")}{(isStruct ? "struct" : "class")} {name} : IPacketSerializable
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