using JetBrains.Annotations;

namespace QuantumCore.Networking;

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public class PacketAttribute : Attribute
{
    public PacketAttribute(byte header, EDirection direction)
    {
        Header = header;
        Direction = direction;
    }

    public byte Header { get; set; }
    public EDirection Direction { get; set; }
    public bool Sequence { get; set; }
}