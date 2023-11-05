using QuantumCore.API.Core.Models;

namespace QuantumCore.API.Data;

public interface IPlayerManager
{
    Task<PlayerData?> GetPlayer(Guid account, byte slot);
    Task<PlayerData> GetPlayer(Guid playerId);
    IAsyncEnumerable<PlayerData> GetPlayers(Guid account);
    Task DeleteAsync(PlayerData player);
}
