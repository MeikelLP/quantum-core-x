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

    public bool HasDynamicLength => (SemanticType.Name == "String" && ElementSize == 0) || 
                                    (IsArray && ArrayLength is null);
    public ITypeSymbol SemanticType { get; set; } = null!;
    public SyntaxNode SyntaxNode { get; set; } = null!;
    public string? SizeFieldName { get; set; }
    public bool IsCustom { get; set; }
    public bool IsRecordParameter { get; set; }
    public bool IsReadonly { get; set; }

    public string GetVariableName()
    {
        return $"__{Name[0].ToString().ToLower()}{Name.Substring(1)}";
    }
}
