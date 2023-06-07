namespace QuantumCore.Networking;

internal class FieldData
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public bool IsArray { get; set; }
    public bool IsEnum { get; set; }
    public int? ArrayLength { get; set; }
    public int ElementSize { get; set; }
    public int? Order { get; set; }

    public int FieldSize => HasDynamicLength 
        ? 0 
        : ElementSize * (ArrayLength ?? 1);

    public bool HasDynamicLength => Type == "string" || (IsArray && ArrayLength == null);
}