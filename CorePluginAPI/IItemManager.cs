using System.Threading;
using System.Threading.Tasks;
using QuantumCore.API.Core.Models;

namespace QuantumCore.API;

public interface IItemManager
{
    ItemData GetItem(uint id);
    Task LoadAsync(CancellationToken token = default);
    ItemInstance CreateItem(ItemData proto, byte count = 1);
}