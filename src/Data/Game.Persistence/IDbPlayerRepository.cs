using QuantumCore.API;
using QuantumCore.API.Core.Models;

namespace QuantumCore.Game.Persistence;

public interface IDbPlayerRepository : IPlayerRepository
{
    Task<PlayerData[]> GetPlayersAsync(Guid accountId);
    Task<bool> IsNameInUseAsync(string name);
    Task CreateAsync(PlayerData player);
    Task DeletePlayerAsync(PlayerData player);
    Task UpdateEmpireAsync(Guid accountId, Guid playerId, byte empire);
    Task SetPlayerAsync(PlayerData data);
}