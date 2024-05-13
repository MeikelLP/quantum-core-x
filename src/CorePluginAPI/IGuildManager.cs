namespace QuantumCore.API;

public interface IGuildManager
{
    Task<GuildData?> GetGuildByNameAsync(string name);
    Task<GuildData?> GetGuildForPlayerAsync(Guid playerId);
    Task<GuildData> CreateGuildAsync(string name, Guid leaderId);
}
