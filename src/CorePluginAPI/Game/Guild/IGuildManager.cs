using System.Collections.Immutable;

namespace QuantumCore.API.Game.Guild;

public interface IGuildManager
{
    Task<GuildData?> GetGuildByNameAsync(string name, CancellationToken token = default);
    Task<GuildData?> GetGuildForPlayerAsync(uint playerId, CancellationToken token = default);
    Task<GuildData> CreateGuildAsync(string name, uint leaderId, CancellationToken token = default);
    Task<uint> CreateNewsAsync(uint guildId, string message, uint playerId, CancellationToken token = default);
    Task<bool> HasPermissionAsync(uint playerId, GuildRankPermission perm, CancellationToken token = default);
    Task<ImmutableArray<GuildNewsData>> GetGuildNewsAsync(uint guildId, CancellationToken token = default);
    Task<bool> DeleteNewsAsync(uint guildId, uint newsId, CancellationToken token = default);
}