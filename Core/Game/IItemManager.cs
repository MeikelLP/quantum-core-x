using System.Threading;
using System.Threading.Tasks;
using QuantumCore.Core.Types;
using QuantumCore.Database;

namespace QuantumCore.Game;

public interface IItemManager
{
    ItemProto.Item GetItem(uint id);
    Task LoadAsync(CancellationToken token = default);
    Item CreateItem(ItemProto.Item proto, byte count = 1);
}