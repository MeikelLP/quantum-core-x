using QuantumCore.Networking;

namespace QuantumCore.API;

public sealed class PacketContext<TConnection, TPacket>
    where TConnection : IConnection
    where TPacket : IPacket
{
    public required TPacket Packet { get; set; }
    public required TConnection Connection { get; set; }
}