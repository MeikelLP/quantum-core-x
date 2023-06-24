using QuantumCore.API;
using QuantumCore.API.Core.Models;

namespace Game.Caching;

public interface ICachePlayerStore : IPlayerStore
{
    Task<PlayerData?> GetPlayerAsync(Guid playerId, byte slot);
    Task SetPlayerAsync(PlayerData player);
    Task CreateAsync(PlayerData player);
    Task DeletePlayerAsync(PlayerData player);
}