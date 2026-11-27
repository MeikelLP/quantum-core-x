using QuantumCore.Game.Types.Guild;
using QuantumCore.Networking;

namespace QuantumCore.Packets.Guild;

[Packet(0x50, EDirection.INCOMING, Sequence = true)]
[SubPacket(0x03, 0)]
[PacketGenerator]
public partial class GuildRankChangePacket
{
    public byte Position { get; set; }
    public GuildRankPermissions Permission { get; set; }
}
