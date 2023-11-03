using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;

namespace QuantumCore.API;

public interface IEquipment
{
    Guid Owner { get; }
    ItemInstance? Body { get; }
    ItemInstance? Head { get; }
    ItemInstance? Shoes { get; }
    ItemInstance? Bracelet { get; }
    ItemInstance? Weapon { get; }
    ItemInstance? Necklace { get; }
    ItemInstance? Earrings { get; }
    ItemInstance? Costume { get; }
    ItemInstance? Hair { get; }
    bool SetItem(ItemInstance item);
    bool SetItem(ItemInstance item, ushort position);
    ItemInstance? GetItem(EquipmentSlots slot);
    ItemInstance? GetItem(ushort position);
    bool RemoveItem(ItemInstance item);
    void Send(IPlayerEntity player);
    bool IsSuitable(IItemManager itemManager, ItemInstance item, ushort position);
    long GetWearPosition(IItemManager itemManager, uint itemId);
}
