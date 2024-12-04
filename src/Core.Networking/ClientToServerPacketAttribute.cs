namespace QuantumCore.Networking;

[AttributeUsage(AttributeTargets.Struct)]
public class ClientToServerPacketAttribute : Attribute
{
    public ClientToServerPacketAttribute(byte header)
    {
        Header = header;
    }

    public ClientToServerPacketAttribute(byte header, byte subHeader)
    {
        Header = header;
        SubHeader = subHeader;
    }

    public byte Header { get; init; }
    public byte? SubHeader { get; init; }
    public bool IsDynamic { get; init; }
    public bool HasSequence { get; init; }
}
