using QuantumCore.Networking;

namespace QuantumCore.Packets;

[Packet(0x2C, EDirection.OUTGOING, Sequence = true)]
[PacketGenerator]
public partial class Ping
{
}
