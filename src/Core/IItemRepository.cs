using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types.Items;

namespace QuantumCore.API;

public interface IItemRepository
{
    Task<IEnumerable<Guid>> GetItemIdsForPlayerAsync(uint playerId, WindowType window);
    Task<ItemInstance?> GetItemAsync(Guid id);
    Task DeletePlayerItemsAsync(uint playerId);
    Task DeletePlayerItemAsync(uint playerId, uint itemId);
    Task SaveItemAsync(ItemInstance item);
}
