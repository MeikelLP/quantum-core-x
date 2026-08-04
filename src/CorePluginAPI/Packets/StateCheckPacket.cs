using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0xCE, EDirection.INCOMING)]
[PacketGenerator]
public partial class StateCheckPacket
{
}
