#nullable enable
using QuantumCore.API.Core.Models;

namespace QuantumCore.API;

public record struct SlotChangedEventArgs(ItemInstance? ItemInstance, EquipmentSlots Slot);