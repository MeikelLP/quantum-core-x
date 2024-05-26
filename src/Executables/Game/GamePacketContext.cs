using QuantumCore.Networking;

namespace QuantumCore.Game;

public class GamePacketContext<TPacket> : PacketContext<GameConnection>
    where TPacket : IPacket
{
    public required TPacket Packet { get; set; }
}