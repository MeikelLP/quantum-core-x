using QuantumCore.API.Core.Models;

namespace QuantumCore.API;

public interface IItemRepository
{
    Task<IEnumerable<Guid>> GetItemIdsForPlayerAsync(Guid player, byte window);
    Task<ItemInstance?> GetItemAsync(Guid id);
    Task DeletePlayerItemsAsync(Guid playerId);
}
