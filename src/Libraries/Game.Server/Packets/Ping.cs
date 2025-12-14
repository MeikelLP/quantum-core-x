using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x2C, EDirection.Outgoing, Sequence = true)]
[PacketGenerator]
public partial class Ping
{
}