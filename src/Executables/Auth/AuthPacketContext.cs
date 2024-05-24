using QuantumCore.Networking;

namespace QuantumCore.Auth;

public class AuthPacketContext<TPacket> : PacketContext<AuthConnection>
    where TPacket : IPacket
{
    public required TPacket Packet { get; set; }
}