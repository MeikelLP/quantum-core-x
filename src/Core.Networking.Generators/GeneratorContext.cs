using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace QuantumCore.Networking;

internal class GeneratorContext
{
    private static SyntaxToken GetFieldName(FieldDeclarationSyntax x)
    {
        return x.Declaration.Variables.First().Identifier;
    }

    private static int? GetArrayLength(PropertyDeclarationSyntax x)
    {
        if (x is
            {
                Type: ArrayTypeSyntax, Initializer: not null, Initializer.Value: ArrayCreationExpressionSyntax
                {
                    Type.RankSpecifiers.Count: 1
                } arrayCreationExpressionSyntax
            } &&
            arrayCreationExpressionSyntax.Type.RankSpecifiers.Any() &&
            arrayCreationExpressionSyntax.Type.RankSpecifiers.First().Sizes.OfType<LiteralExpressionSyntax>().Any())
        {
            return (int?) arrayCreationExpressionSyntax.Type.RankSpecifiers
                .First().Sizes.OfType<LiteralExpressionSyntax>()
                .First().Token.Value;
        }

        return null;
    }

    private static int? GetArrayLength(BaseFieldDeclarationSyntax x)
    {
        if (x.Declaration.Type is ArrayTypeSyntax &&
            x.Declaration.Variables.First().Initializer?.Value is ArrayCreationExpressionSyntax
            {
                Type.RankSpecifiers.Count: 1
            } arrayCreationExpressionSyntax)
        {
            var size = arrayCreationExpressionSyntax.Type.RankSpecifiers.First()
                .Sizes.OfType<LiteralExpressionSyntax>()
                .FirstOrDefault();
            return (int?) size?.Token.Value;
        }

        return null;
    }
}