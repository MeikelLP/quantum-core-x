using System.Diagnostics;

namespace QuantumCore.Networking;

[DebuggerDisplay("{TypeFullName} {FieldName}")]
internal class FieldData
{
    public string FieldName { get; set; } = "";
    public bool IsArray { get; set; }
    public bool IsEnum { get; set; }
    public int? ArrayLength { get; set; }
    public int ElementSize { get; set; }
    public string? ElementTypeFullName { get; set; }
    public FieldData[]? ElementTypeFields { get; set; }

    public int FieldSize => HasDynamicLength
        ? 0
        : ElementSize * (ArrayLength ?? 1);

    public bool HasDynamicLength => (TypeFullName == "System.String" && ElementSize == 0) ||
                                    (IsArray && ArrayLength == null);

    public string? SizeFieldName { get; set; }
    public bool IsCustom { get; set; }
    public bool IsReadonly { get; set; }
    public string TypeFullName { get; set; } = "";
    public FieldData[]? SubFields { get; set; }

    public string GetVariableName()
    {
        return $"__{FieldName[0].ToString().ToLower()}{FieldName.Substring(1)}";
    }
}