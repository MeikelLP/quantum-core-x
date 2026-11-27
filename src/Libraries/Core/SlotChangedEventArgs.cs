using QuantumCore.Game.Types.Items;
using QuantumCore.Models;

namespace QuantumCore;

public record struct SlotChangedEventArgs(ItemInstance? ItemInstance, EquipmentSlot Slot);
