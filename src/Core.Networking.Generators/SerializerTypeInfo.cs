using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace QuantumCore.Networking;

internal readonly struct SerializerTypeInfo
{
    public readonly string Name;
    public readonly string? FullName;
    public readonly string Namespace;
    public readonly Location? Location;
    public readonly string Modifiers;
    public readonly ImmutableArray<AttributeData> Attributes;
    public readonly ImmutableArray<FieldData> Fields;

    public SerializerTypeInfo(SemanticModel semanticModel, TypeDeclarationSyntax typeDeclarationSyntax)
    {
        var declaredSymbol = (INamedTypeSymbol) semanticModel.GetDeclaredSymbol(typeDeclarationSyntax)!;
        Name = declaredSymbol.Name;
        FullName = declaredSymbol.GetFullName();
        Namespace = declaredSymbol.GetFullNamespace()!;
        Location = typeDeclarationSyntax.GetLocation();
        Modifiers = string.Join(" ", typeDeclarationSyntax.Modifiers.Select(x => x.Text));
        Attributes = declaredSymbol.GetAttributes();
        Fields = declaredSymbol
            .GetMembers()
            .OfType<IFieldSymbol>()
            .Where(x => x.DeclaredAccessibility == Accessibility.Public)
            .Select(member => GeneratorConstants.BuildFieldData(semanticModel, member))
            .Concat(declaredSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(x => x.DeclaredAccessibility == Accessibility.Public)
                .Select(member => GeneratorConstants.BuildFieldData(semanticModel, member)))
            .ToImmutableArray();
    }

    public SerializerTypeInfo(SemanticModel semanticModel, INamedTypeSymbol declaredSymbol)
    {
        Name = declaredSymbol.Name;
        FullName = declaredSymbol.GetFullName();
        Namespace = declaredSymbol.GetFullNamespace()!;
        Location = null; // unknown at this point
        Modifiers = ""; // No modifiers because they cannot be fully extracted from here
        Attributes = declaredSymbol.GetAttributes();
        Fields = declaredSymbol
            .GetMembers()
            .OfType<IFieldSymbol>()
            .Where(x => x.DeclaredAccessibility == Accessibility.Public)
            .Select(member => GeneratorConstants.BuildFieldData(semanticModel, member))
            .ToImmutableArray();
    }


    public static TypeDeclarationSyntax GetTypeDeclaration(ITypeSymbol semanticType)
    {
        return semanticType.DeclaringSyntaxReferences
            .Select(x => x.GetSyntax())
            .OfType<TypeDeclarationSyntax>()
            .First();
    }

    internal static int GetStaticSizeOfType(IReadOnlyList<FieldData> fields)
    {
        return fields.Sum(x => x.FieldSize);
    }
}