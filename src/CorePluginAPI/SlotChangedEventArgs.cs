#nullable enable
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types.Items;

namespace QuantumCore.API;

public record struct SlotChangedEventArgs(ItemInstance? ItemInstance, EquipmentSlot Slot);