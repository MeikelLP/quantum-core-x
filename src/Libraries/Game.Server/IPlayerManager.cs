using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.Types.Players;

namespace QuantumCore.Game;

public interface IPlayerManager
{
    /// <summary>
    /// Tries to get a player by account ID + slot
    /// First tries to load player from cache then from database.
    /// If player is not found in cache but in database: The cache will be updated.
    /// </summary>
    Task<PlayerData?> GetPlayer(Guid accountId, byte slot);

    /// <summary>
    /// Tries to get a player by player ID
    /// First tries to load player from cache then from database.
    /// If player is not found in cache but in database: The cache will be updated.
    /// </summary>
    Task<PlayerData?> GetPlayer(uint playerId);

    /// <summary>
    /// Gets all players for account ID
    /// </summary>
    Task<PlayerData[]> GetPlayers(Guid accountId);

    Task<bool> IsNameInUseAsync(string name);

    /// <summary>
    /// Creates a new player and persists it in the database and the cache
    /// </summary>
    /// <returns>The slot of the newly created player</returns>
    Task<PlayerData> CreateAsync(Guid accountId, string playerName, EPlayerClassGendered @class, byte appearance);

    Task DeletePlayerAsync(PlayerData player);
    Task SetPlayerEmpireAsync(Guid accountId, uint playerId, EEmpire empire);
    Task SetPlayerAsync(PlayerData data);
}
