using Microsoft.CodeAnalysis;

namespace QuantumCore.Networking;

internal class SerializerTypeInfo
{
    public INamedTypeSymbol Symbol { get; }
    public SyntaxNode Node { get; }
    public SemanticModel SemanticModel { get; }

    public SerializerTypeInfo(INamedTypeSymbol symbol, SyntaxNode node, SemanticModel semanticModel)
    {
        Symbol = symbol;
        Node = node;
        SemanticModel = semanticModel;
    }
}