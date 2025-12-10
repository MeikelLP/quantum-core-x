using System.Collections.ObjectModel;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types.Items;

namespace QuantumCore.API;

public interface IInventory
{
    uint Owner { get; }
    WindowType Window { get; }
    ReadOnlyCollection<ItemInstance> Items { get; }
    IEquipment EquipmentWindow { get; }
    long Size { get; }
    event EventHandler<SlotChangedEventArgs> OnSlotChanged;
    Task Load();
    Task<bool> PlaceItem(ItemInstance item);
    Task<bool> PlaceItem(ItemInstance item, ushort position);
    void RemoveItem(ItemInstance item);
    ItemInstance? GetItem(ushort position);
    bool IsSpaceAvailable(ItemInstance item, ushort position);
    void MoveItem(ItemInstance item, ushort fromPosition, ushort position);
    void SetEquipment(ItemInstance item, ushort position);
    void RemoveEquipment(ItemInstance item);
}
