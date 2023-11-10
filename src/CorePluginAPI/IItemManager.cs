using QuantumCore.API.Core.Models;

namespace QuantumCore.API;

public interface IItemManager
{
    ItemData? GetItem(uint id);
    ItemData? GetItemByName(ReadOnlySpan<char> name);
    Task LoadAsync(CancellationToken token = default);
    ItemInstance CreateItem(ItemData proto, byte count = 1);
}
