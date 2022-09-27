using System;
using System.Threading.Tasks;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;

namespace Core.Tests;

public static class InventoryUtils
{
    public static async ValueTask AddItem(this IPlayerEntity player, uint itemProtoId, byte count)
    {
        await player.Inventory.PlaceItem(new ItemInstance
        {
            Count = count,
            Id = Guid.NewGuid(),
            ItemId = itemProtoId
        });
    }
}