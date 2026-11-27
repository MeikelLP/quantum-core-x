using QuantumCore.Networking;

namespace QuantumCore.API.Packets;

[Packet(0x2C, EDirection.OUTGOING, Sequence = true)]
[PacketGenerator]
public partial class Ping
{
}
