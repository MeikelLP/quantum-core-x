using QuantumCore.API.Game.Guild;
using QuantumCore.API.Game.Types.Guild;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Guild;

[Packet(0x4B, EDirection.OUTGOING)]
[SubPacket(0x03, 1)]
[PacketGenerator]
public partial class GuildRankPacket
{
    [Field(0)] public ushort Size => (ushort) Ranks.Length;
    [Field(1)] public byte Length { get; set; } = GuildConstants.RANKS_LENGTH;

    [Field(2, ArrayLength = GuildConstants.RANKS_LENGTH)]
    public GuildRankDataPacket[] Ranks { get; set; } = [];
}

public class GuildRankDataPacket
{
    [Field(0)] public byte Rank { get; set; }
    [Field(1, Length = 9)] public string Name { get; set; } = "";
    [Field(2)] public GuildRankPermissions Permissions { get; set; }
}
