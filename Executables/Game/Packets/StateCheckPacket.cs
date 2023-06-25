using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0xCE, EDirection.Incoming)]
[PacketGenerator]
public partial class StateCheckPacket
{
}