namespace QuantumCore.Networking;

internal record struct PacketTypeInfo(
    string Name,
    string Namespace,
    string Modifiers,
    IReadOnlyList<FieldData> Fields)
{
    public bool HasDynamicLength => Fields.Any(x => x.HasDynamicLength);

    public int? StaticSize => HasDynamicLength
        ? null
        : Fields.Sum(x => x.FieldSize);
}