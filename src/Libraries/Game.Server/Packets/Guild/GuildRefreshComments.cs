using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Guild;

[PacketGenerator]
[Packet(0x50, EDirection.Incoming, Sequence = true)]
[SubPacket(0x07, 0)]
public partial class GuildRefreshComments
{
}