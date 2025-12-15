using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0xFE, EDirection.INCOMING /*, Sequence = true*/)] // sequence only when connection is encrypted - how?
[PacketGenerator]
public partial class Pong
{
}
