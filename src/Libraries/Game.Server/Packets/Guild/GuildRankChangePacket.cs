using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Guild;

[Packet(0x50, EDirection.Incoming, Sequence = true)]
[SubPacket(0x03, 0)]
[PacketGenerator]
public partial class GuildRankChangePacket
{
    public byte Position { get; set; }
    public byte Permission { get; set; }
}
