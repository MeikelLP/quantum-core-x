using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace QuantumCore.Networking;

[Generator(LanguageNames.CSharp)]
public class PacketSerializerGenerator : IIncrementalGenerator
{
    private SerializeGenerator _serializeGenerator = null!;
    private DeserializeGenerator _deserializeGenerator = null!;
    private GeneratorContext _generatorContext = null!;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assemblyName = context.CompilationProvider.Select(static (c, _) => c.AssemblyName);
        var sourceFiles = context.SyntaxProvider
            .ForAttributeWithMetadataName(GeneratorConstants.PACKETGENEREATOR_ATTRIBUTE_FULLNAME,
                CouldBeEnumerationAsync, GetTypeInfo)
            .Collect()
            .SelectMany((info, _) => info.Distinct());

        var combined = sourceFiles.Combine(assemblyName);

        context.RegisterSourceOutput(combined, (spc, compilationPair) =>
        {
            var (typeInfo, assembly) = compilationPair;
            _generatorContext = new GeneratorContext(typeInfo);
            _serializeGenerator = new SerializeGenerator(_generatorContext);
            _deserializeGenerator = new DeserializeGenerator(_generatorContext);
            var typeDeclarationSyntax = (TypeDeclarationSyntax) _generatorContext.Type.Node;
            try
            {
                var (name, source) = GenerateFile(typeDeclarationSyntax, _generatorContext.Type.Node.SyntaxTree);

                spc.AddSource($"{name}.g.cs", SourceText.From(source, Encoding.UTF8));
            }
            catch (DiagnosticException e)
            {
                spc.ReportDiagnostic(e.Diagnostic);
            }
            catch (Exception e)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "QCX000001",
                        "Failed to generate packet serializer",
                        "Type {0} is setup incorrectly. Exception: {1} => {2}",
                        "generators",
                        DiagnosticSeverity.Error,
                        true), typeDeclarationSyntax.GetLocation(), typeDeclarationSyntax.Identifier.Text,
                    e.GetType(), e.Message));
            }
        });
    }

    private (string Name, string Source) GenerateFile(TypeDeclarationSyntax type, SyntaxTree tree)
    {
        var name = type.Identifier.Text;
        var ns = tree.GetRoot().DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>().First()?.Name.ToString()!;
        var typeKeyWords = GeneratorContext.GetTypeKeyWords(type);
        var packetAttr = type.AttributeLists
            .SelectMany(x => x.Attributes)
            .First(x => ((IdentifierNameSyntax) x.Name).Identifier.Text == "Packet");
        var header = ((LiteralExpressionSyntax) packetAttr.ArgumentList!.Arguments[0].Expression).Token.Text;
        var subPacketAttr = type.AttributeLists
            .SelectMany(x => x.Attributes)
            .FirstOrDefault(x => ((IdentifierNameSyntax) x.Name).Identifier.Text == "SubPacket");
        var subHeader = (subPacketAttr?.ArgumentList?.Arguments.FirstOrDefault()?.Expression as LiteralExpressionSyntax)
            ?.Token.Text ?? "null";
        var hasStaticSize = _generatorContext.GetFieldsOfType(type).All(x => !x.HasDynamicLength);
        var hasSequence = packetAttr.ArgumentList.Arguments.Any(x =>
            x.NameEquals?.Name.Identifier.Text == "Sequence" &&
            ((LiteralExpressionSyntax) x.Expression).Token.Text == "true"); //packetAttr.ArgumentList
        var source = new StringBuilder();
        ApplyHeader(source, typeKeyWords, ns, name, header, subHeader, hasStaticSize, hasSequence);

        var dynamicByteIndex = new StringBuilder();
        source.Append(_serializeGenerator.Generate(type, dynamicByteIndex));

        source.AppendLine();

        dynamicByteIndex = new StringBuilder();
        source.Append(_deserializeGenerator.Generate(type, dynamicByteIndex.ToString()));

        ApplyFooter(source);
        return (name, source.ToString());
    }

    private static void ApplyFooter(StringBuilder source)
    {
        source.AppendLine("    }");
        source.Append("}");
    }

    // TODO deserialize


    private static bool CouldBeEnumerationAsync(
        SyntaxNode syntaxNode,
        CancellationToken cancellationToken)
    {
        return syntaxNode is StructDeclarationSyntax or ClassDeclarationSyntax &&
               IsPartial((TypeDeclarationSyntax) syntaxNode);
    }

    private static bool IsPartial(TypeDeclarationSyntax declaration)
    {
        return declaration.Modifiers.Any(x => x.Text == "partial");
    }

    private static SerializerTypeInfo GetTypeInfo(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken)
    {
        return new SerializerTypeInfo((INamedTypeSymbol) context.TargetSymbol, context.TargetNode,
            context.SemanticModel);
    }

    private static void ApplyHeader(StringBuilder sb, string typeKeywords, string ns, string name, string header,
        string subHeader, bool hasStaticSize, bool hasSequence)
    {
        sb.AppendLine($@"/// <auto-generated/>
using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;
using QuantumCore.Networking;

// no async warning if no properties
#pragma warning disable CS1998

namespace {ns} {{

    public partial {typeKeywords} {name} : IPacketSerializable
    {{
        public byte Header => {header};
        public byte? SubHeader => {subHeader};
        public bool HasStaticSize => {hasStaticSize.ToString().ToLower()};
        public bool HasSequence => {hasSequence.ToString().ToLower()};
");
    }
}