using System.Collections.Immutable;

namespace QuantumCore.API.Game.Guild;

public class GuildData
{
    public uint Id { get; set; }
    public string Name { get; set; } = "";
    public uint OwnerId { get; set; }
    public byte Level { get; set; }
    public uint Experience { get; set; }
    public ushort MaxMemberCount { get; set; }
    public uint Gold { get; set; }
    public ImmutableArray<GuildMemberData> Members { get; set; }
    public ImmutableArray<GuildRankData> Ranks { get; set; }
}