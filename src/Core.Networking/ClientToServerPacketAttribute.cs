namespace QuantumCore.Networking;

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
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

    public byte Header { get; set; }
    public byte? SubHeader { get; set; }
    public bool HasSequence { get; set; }
}