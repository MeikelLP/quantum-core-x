namespace QuantumCore.Networking;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class PacketAttribute : Attribute
{
    public PacketAttribute(byte header, EDirection direction)
    {
        Header = header;
        Direction = direction;
    }

    public byte Header { get; }
    public EDirection Direction { get; }
    public bool Sequence { get; set; }
}