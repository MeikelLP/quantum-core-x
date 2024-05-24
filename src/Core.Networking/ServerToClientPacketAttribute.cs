namespace QuantumCore.Networking;

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public class ServerToClientPacketAttribute : Attribute
{
    public ServerToClientPacketAttribute(byte header)
    {
        Header = header;
    }

    public ServerToClientPacketAttribute(byte header, byte subHeader)
    {
        Header = header;
        SubHeader = subHeader;
    }

    public byte Header { get; set; }
    public byte? SubHeader { get; set; }
    public bool HasSequence { get; set; }
}