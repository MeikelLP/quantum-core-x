using QuantumCore.API.Game.Types.Guild;

namespace QuantumCore.API.Game.Guild;

public class GuildRankData
{
    public byte Position { get; set; }
    public string Name { get; set; } = "";
    public GuildRankPermissions Permissions { get; set; }
}
