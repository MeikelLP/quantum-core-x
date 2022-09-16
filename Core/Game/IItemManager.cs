using QuantumCore.Core.Types;
using QuantumCore.Database;

namespace QuantumCore.Game;

public interface IItemManager
{
    ItemProto.Item GetItem(uint id);
    void Load();
    Item CreateItem(ItemProto.Item proto, byte count = 1);
}