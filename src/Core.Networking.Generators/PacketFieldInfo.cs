namespace QuantumCore.Networking;

public class PacketFieldInfo
{
    public string Name { get; set; } = "";
    public bool IsArray { get; set; }
    public bool IsEnum { get; set; }
    public int? ArrayLength { get; set; }
    public int ElementSize { get; set; }
    public int? Order { get; set; }
    public string TypeFullName { get; set; } = "";

    public int FieldSize => HasDynamicLength
        ? 0
        : ElementSize * (ArrayLength ?? 1);

    public bool HasDynamicLength => (TypeFullName == typeof(string).FullName && ElementSize == 0) ||
                                    (IsArray && ArrayLength == null);

    public string? SizeFieldName { get; set; }
    public bool IsCustom { get; set; }
    public bool IsRecordParameter { get; set; }
    public bool IsReadonly { get; set; }
}
