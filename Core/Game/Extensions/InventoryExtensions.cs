using System;
using System.Linq;
using System.Threading.Tasks;
using QuantumCore.API;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Cache;
using QuantumCore.Extensions;
using QuantumCore.Game.Constants;

namespace QuantumCore.Game.Extensions;

public static class InventoryExtensions
{
    /// <summary>
    /// Adds or removes quantities of the given item proto. Does only work for stackable items.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="itemId">ID of the item (proto)</param>
    /// <param name="amount">If positive adds items. If negative remove items.</param>
    public static async ValueTask<bool> AddItemAmountAsync(this IPlayerEntity player, IItemManager itemManager, ICacheManager cacheManager, uint itemId, short amount)
    {
        ArgumentNullException.ThrowIfNull(player);
        
        if (amount == 0) return false;
        
        var itemProto = itemManager.GetItem(itemId);
        if (!((EItemFlags) itemProto.Flags).HasFlag(EItemFlags.ITEM_FLAG_STACKABLE))
        {
            // TODO handle non stackable items
            return false;
        }

        var remainingAmount = amount;
        
        while (remainingAmount != 0)
        {
            short changed;
            if (remainingAmount > 0)
            {
                var existingItem = player.Inventory.Items.LastOrDefault(x => x.ItemId == itemId && x.Count != InventoryConstants.MAX_STACK_SIZE);
                if (existingItem is not null)
                {
                    // modify existing item if existent
                    var newValue = (byte) Math.Clamp(existingItem.Count + amount, 0, InventoryConstants.MAX_STACK_SIZE);
                    changed = (short)(newValue - existingItem.Count);
                    existingItem.Count = newValue;
                    await player.SendItem(existingItem);
                    await existingItem.Persist(cacheManager);
                }
                else
                {
                    var newValue = (byte)Math.Clamp(remainingAmount, (short)0, InventoryConstants.MAX_STACK_SIZE);
                    // add more items to inventory
                    var newItem = itemManager.CreateItem(itemProto, newValue);
                    await player.Inventory.PlaceItem(newItem);
                    await player.SendItem(newItem);
                    await newItem.Persist(cacheManager);
                
                    changed = newValue;
                }
            }
            else
            {
                var existingItem = player.Inventory.Items.LastOrDefault(x => x.ItemId == itemId);
                if (existingItem is not null)
                {
                    var newValue = (byte)Math.Clamp(existingItem.Count + remainingAmount, 0, InventoryConstants.MAX_STACK_SIZE);
                    if (newValue == 0)
                    {
                        // remove items from inventory
                        player.Inventory.RemoveItem(existingItem);
                        await existingItem.Set(cacheManager, Guid.Empty, existingItem.Window, existingItem.Position);
                        await existingItem.Destroy(cacheManager);
                        await player.SendRemoveItem(existingItem.Window, (ushort)existingItem.Position);
                        changed = (short)-existingItem.Count;
                    }
                    else
                    {
                        // decrease item count
                        changed = (short)(newValue - existingItem.Count);
                        existingItem.Count = newValue;
                        await player.SendItem(existingItem);
                        await existingItem.Persist(cacheManager);
                    }
                }
                else
                {
                    // if no more items to remove set changed to remainingAmount to break the loop
                    changed = remainingAmount;
                }
            }

            remainingAmount -= changed;
        }
        return true;
    }
}