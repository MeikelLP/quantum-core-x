using Microsoft.CodeAnalysis;

namespace QuantumCore.Networking;

public static class Extensions
{
    public static string? GetFullName(this ITypeSymbol? type)
    {
        if (type is null) return null;
        return $"{type.GetFullNamespace()}.{type.Name}";
    }

    public static string? GetFullNamespace(this ITypeSymbol? type)
    {
        if (type is null) return null;
        var namespaces = new List<string>();
        var ns = type.ContainingNamespace;
        while (ns is {Name: not ""})
        {
            namespaces.Add(ns.Name);
            ns = ns.ContainingNamespace;
        }

        namespaces.Reverse();
        return string.Join(".", namespaces);
    }
}