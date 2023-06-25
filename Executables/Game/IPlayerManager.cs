using QuantumCore.API.Core.Models;

namespace QuantumCore.Game;

public interface IPlayerManager
{
    Task<PlayerData?> GetPlayer(Guid account, byte slot);
    Task<PlayerData?> GetPlayer(Guid playerId);
    Task<PlayerData[]> GetPlayers(Guid account);
    Task<bool> IsNameInUseAsync(string name);
    
    /// <summary>
    /// Creates a new player and persists it in the database and the cache
    /// </summary>
    /// <returns>The slot of the newly created player</returns>
    Task<PlayerData> CreateAsync(Guid accountId, string playerName, byte @class, byte appearance);

    Task DeletePlayerAsync(PlayerData player);
}