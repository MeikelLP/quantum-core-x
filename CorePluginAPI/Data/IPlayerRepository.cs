using QuantumCore.API.Core.Models;

namespace QuantumCore.API.Data;

public interface IPlayerRepository
{
    Task DeleteAsync(PlayerData player);
    Task<PlayerData> GetPlayerAsync(Guid playerId);
    Task<IEnumerable<Guid>> GetPlayerIdsForAccountAsync(Guid account);
}
