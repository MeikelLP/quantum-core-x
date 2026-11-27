using QuantumCore.Networking;

namespace QuantumCore.Packets;

[Packet(0x0a, EDirection.INCOMING, Sequence = true)]
[PacketGenerator]
public partial class EnterGame
{
}
