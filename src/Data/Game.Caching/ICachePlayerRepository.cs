using QuantumCore;
using QuantumCore.Game.Types;
using QuantumCore.Models;

namespace Game.Caching;

public interface ICachePlayerRepository : IPlayerRepository
{
    Task<PlayerData?> GetPlayerAsync(Guid accountId, byte slot);
    Task SetPlayerAsync(PlayerData player);
    Task CreateAsync(PlayerData player);
    Task DeletePlayerAsync(PlayerData player);

    /// <summary>
    /// Gets the selected empire by the user for creating and persisting the player character.
    /// </summary>
    Task<EEmpire?> GetTempEmpireAsync(Guid accountId);

    /// <summary>
    /// Cache the users selected empire for usage when creating and persisting the player character.
    /// </summary>
    Task SetTempEmpireAsync(Guid accountId, EEmpire empire);
}
