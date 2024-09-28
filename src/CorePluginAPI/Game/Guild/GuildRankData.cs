namespace QuantumCore.API.Game.Guild;

public class GuildRankData
{
    public byte Position { get; set; }
    public string Name { get; set; } = "";
    public GuildRankPermission Permissions { get; set; }
}