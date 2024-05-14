namespace QuantumCore.API;

public interface IGuildManager
{
    Task<GuildData?> GetGuildByNameAsync(string name);
    Task<GuildData?> GetGuildForPlayerAsync(uint playerId);
    Task<GuildData> CreateGuildAsync(string name, uint leaderId);
}