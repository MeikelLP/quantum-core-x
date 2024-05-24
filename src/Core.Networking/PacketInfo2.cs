namespace QuantumCore.Networking;

public class PacketInfo2
{
    public bool HasSequence { get; init; }
    public int? StaticSize { get; init; }
    public Type? HandlerType { get; init; }
    public required Type PacketType { get; init; }
    public PacketHeaderDefinition Header { get; init; }
}