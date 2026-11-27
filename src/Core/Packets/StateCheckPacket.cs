using QuantumCore.Networking;

namespace QuantumCore.API.Packets;

[Packet(0xCE, EDirection.INCOMING)]
[PacketGenerator]
public partial class StateCheckPacket
{
}
