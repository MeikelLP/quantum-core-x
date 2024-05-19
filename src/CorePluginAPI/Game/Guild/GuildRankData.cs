namespace QuantumCore.API.Game.Guild;

public class GuildRankData
{
    public byte Rank { get; set; }
    public string Name { get; set; } = "";
    public GuildRankPermission Permissions { get; set; }
}