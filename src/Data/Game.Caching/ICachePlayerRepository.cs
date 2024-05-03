using QuantumCore.API;
using QuantumCore.API.Core.Models;

namespace Game.Caching;

public interface ICachePlayerRepository : IPlayerRepository
{
    Task<PlayerData?> GetPlayerAsync(Guid playerId, byte slot);
    Task SetPlayerAsync(PlayerData player, byte slot);
    Task CreateAsync(PlayerData player);
    Task DeletePlayerAsync(PlayerData player);

    /// <summary>
    /// Gets the selected empire by the user for creating and persisting the player character.
    /// </summary>
    Task<byte?> GetTempEmpireAsync(Guid accountId);

    /// <summary>
    /// Cache the users selected empire for usage when creating and persisting the player character.
    /// </summary>
    Task SetTempEmpireAsync(Guid accountId, byte empire);
}
