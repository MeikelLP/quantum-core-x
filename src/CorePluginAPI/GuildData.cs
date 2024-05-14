using System.Collections.Immutable;

namespace QuantumCore.API;

public class GuildData
{
    public uint Id { get; set; }
    public string Name { get; set; } = "";
    public uint LeaderId { get; set; }
    public byte Level { get; set; }
    public uint Experience { get; set; }
    public ushort MaxMemberCount { get; set; }
    public uint Gold { get; set; }
    public ImmutableArray<GuildMemberData> Members { get; set; }
}