using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace QuantumCore.Networking;

[DebuggerDisplay("{SemanticType.Name} {Name}")]
internal class FieldData
{
    public string Name { get; set; } = "";
    public bool IsArray { get; set; }
    public bool IsEnum { get; set; }
    public int? ArrayLength { get; set; }
    public int ElementSize { get; set; }
    public int? Order { get; set; }

    public int FieldSize => HasDynamicLength 
        ? 0 
        : ElementSize * (ArrayLength ?? 1);

    public bool HasDynamicLength => SemanticType.Name == "String" || (IsArray && ArrayLength == null);
    public ITypeSymbol SemanticType { get; set; }
    public bool IsCustom { get; set; }
}