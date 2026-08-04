using QuantumCore.Networking;

namespace QuantumCore.API.Packets.Guild;

[PacketGenerator]
[Packet(0x50, EDirection.INCOMING, Sequence = true)]
[SubPacket(0x07, 0)]
public partial class GuildRefreshComments
{
}
