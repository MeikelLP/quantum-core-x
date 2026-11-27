using QuantumCore.Networking;

namespace QuantumCore.Packets;

[Packet(0xCE, EDirection.INCOMING)]
[PacketGenerator]
public partial class StateCheckPacket
{
}
