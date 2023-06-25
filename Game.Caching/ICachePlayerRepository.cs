using QuantumCore.API;
using QuantumCore.API.Core.Models;

namespace Game.Caching;

public interface ICachePlayerRepository : IPlayerRepository
{
    Task<PlayerData?> GetPlayerAsync(Guid playerId, byte slot);
    Task SetPlayerAsync(PlayerData player);
    Task CreateAsync(PlayerData player);
    Task DeletePlayerAsync(PlayerData player);
}