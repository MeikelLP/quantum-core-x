using QuantumCore.API.Game.Guild;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Guild;

[Packet(0x50, EDirection.Incoming, Sequence = true)]
[SubPacket(0x02, 0)]
[PacketGenerator]
public partial class GuildRenameRank
{
    /// <summary>
    /// 1-based
    /// </summary>
    [Field(0)]
    public byte Position { get; set; }

    [Field(1, Length = GuildConstants.RANK_NAME_MAX_LENGTH + 1)]
    public string Name { get; set; } = "";
}