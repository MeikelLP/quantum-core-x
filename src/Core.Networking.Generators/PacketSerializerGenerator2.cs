using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace QuantumCore.Networking;

[Generator(LanguageNames.CSharp)]
public class PacketSerializerGenerator2 : IIncrementalGenerator
{
    public List<PacketTypeInfo> PacketTypes { get; } = new();

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var serverToClientTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName(GeneratorConstants.SERVER_TO_CLIENT_ATTRIBUTE_FULLNAME, Filter,
                (syntaxContext, token) => CollectTypeInfo(true, syntaxContext, token))
            .Collect();
        var clientToServerTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName(GeneratorConstants.CLIENT_TO_SERVER_ATTRIBUTE_FULLNAME, Filter,
                (syntaxContext, token) => CollectTypeInfo(false, syntaxContext, token))
            .Collect();

        var combined = serverToClientTypes.Combine(clientToServerTypes);

        context.RegisterSourceOutput(combined, (spc, types) =>
        {
            var diagnostics = types.Left.Concat(types.Right).SelectMany(x => x.Diagnostics).ToArray();
            if (diagnostics.Length > 0)
            {
                foreach (var diagnostic in diagnostics)
                {
                    spc.ReportDiagnostic(diagnostic);
                }
            }

            if (diagnostics.Any(x => x.Severity == DiagnosticSeverity.Error)) return;

            // only for validating the build type info in tests
            PacketTypes.AddRange(types.Left);
            PacketTypes.AddRange(types.Right);
        });
    }

    private static bool Filter(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        return syntaxNode is StructDeclarationSyntax node && IsPartial(node);
    }

    private static bool IsPartial(TypeDeclarationSyntax declaration)
    {
        return declaration.Modifiers.Any(x => x.Text == "partial");
    }

    private static PacketTypeInfo CollectTypeInfo(bool isServerToClient, GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken)
    {
        return new PacketTypeInfo(isServerToClient, (INamedTypeSymbol)context.TargetSymbol, context.TargetNode,
            context.SemanticModel);
    }
}
