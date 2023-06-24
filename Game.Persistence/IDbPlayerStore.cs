using QuantumCore.API;
using QuantumCore.API.Core.Models;

namespace QuantumCore.Game.Persistence;

public interface IDbPlayerStore : IPlayerStore
{
    Task<PlayerData[]> GetPlayersAsync(Guid accountId);
    Task<bool> IsNameInUseAsync(string name);
    Task CreateAsync(PlayerData player);
    Task DeletePlayerAsync(PlayerData player);
}