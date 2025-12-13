using QuantumCore.API.Game.Types.Players;

namespace QuantumCore.API.Game.Guild;

public class GuildMemberData
{
    public uint Id { get; set; }
    public string Name { get; set; } = "";
    public byte Level { get; set; }
    public EPlayerClassGendered Class { get; set; }
    public bool IsLeader { get; set; }
    public byte Rank { get; set; }
    public uint SpentExperience { get; set; }
}
