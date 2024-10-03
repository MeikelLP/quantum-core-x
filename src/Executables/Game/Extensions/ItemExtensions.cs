using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.Caching;
using QuantumCore.Game.Persistence;
using QuantumCore.Game.PlayerUtils;

namespace QuantumCore.Game.Extensions;

public static class ItemExtensions
{
    public static uint GetMinWeaponBaseDamage(this ItemData item)
    {
        return (uint) item.Values[3];
    }

    public static uint GetMaxWeaponBaseDamage(this ItemData item)
    {
        return (uint) item.Values[4];
    }

    public static uint GetMinMagicWeaponBaseDamage(this ItemData item)
    {
        return (uint) item.Values[1];
    }

    public static uint GetMaxMagicWeaponBaseDamage(this ItemData item)
    {
        return (uint) item.Values[2];
    }

    /// <summary>
    /// Weapon damage added additionally to the base damage
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static uint GetAdditionalWeaponDamage(this ItemData item)
    {
        return (uint) item.Values[5];
    }

    public static uint GetMinWeaponDamage(this ItemData item)
    {
        return item.GetMinWeaponBaseDamage() + item.GetAdditionalWeaponDamage();
    }

    public static uint GetMaxWeaponDamage(this ItemData item)
    {
        return item.GetMaxWeaponBaseDamage() + item.GetAdditionalWeaponDamage();
    }

    public static uint GetMinMagicWeaponDamage(this ItemData item)
    {
        return item.GetMinMagicWeaponBaseDamage() + item.GetAdditionalWeaponDamage();
    }

    public static uint GetMaxMagicWeaponDamage(this ItemData item)
    {
        return item.GetMaxMagicWeaponBaseDamage() + item.GetAdditionalWeaponDamage();
    }

    public static EquipmentSlots? GetWearSlot(this IItemManager itemManager, uint itemId)
    {
        var proto = itemManager.GetItem(itemId);
        if (proto == null)
        {
            return null;
        }

        var wearFlags = (EWearFlags) proto.WearFlags;

        if (wearFlags.HasFlag(EWearFlags.Head))
        {
            return EquipmentSlots.Head;
        }

        if (wearFlags.HasFlag(EWearFlags.Shoes))
        {
            return EquipmentSlots.Shoes;
        }

        if (wearFlags.HasFlag(EWearFlags.Bracelet))
        {
            return EquipmentSlots.Bracelet;
        }

        if (wearFlags.HasFlag(EWearFlags.Weapon))
        {
            return EquipmentSlots.Weapon;
        }

        if (wearFlags.HasFlag(EWearFlags.Necklace))
        {
            return EquipmentSlots.Necklace;
        }

        if (wearFlags.HasFlag(EWearFlags.Earrings))
        {
            return EquipmentSlots.Earring;
        }

        if (wearFlags.HasFlag(EWearFlags.Body))
        {
            return EquipmentSlots.Body;
        }

        return null;
    }

    public static async Task<ItemInstance?> GetItem(this IItemRepository repository, ICacheManager cacheManager,
        Guid id)
    {
        var key = "item:" + id;

        if (await cacheManager.Exists(key) > 0)
        {
            return await cacheManager.Get<ItemInstance>(key);
        }

        var item = await repository.GetItemAsync(id);
        await cacheManager.Set(key, item);
        return item;
    }

    public static async Task DeletePlayerItemAsync(this IItemRepository repository, ICacheManager cacheManager,
        uint playerId, uint itemId)
    {
        var key = $"item:{itemId]";

        await cacheManager.Del(key);

        await repository.DeletePlayerItemAsync(playerId, itemId);
    }

    public static async IAsyncEnumerable<ItemInstance> GetItems(this IItemRepository repository,
        ICacheManager cacheManager, uint player, byte window)
    {
        var key = "items:" + player + ":" + window;

        var list = cacheManager.CreateList<Guid>(key);

        // Check if the window list exists
        if (await cacheManager.Exists(key) > 0)
        {
            var itemIds = await list.Range(0, -1);

            foreach (var id in itemIds)
            {
                var item = await GetItem(repository, cacheManager, id);
                if (item is not null)
                {
                    yield return item;
                }
            }
        }
        else
        {
            var ids = await repository.GetItemIdsForPlayerAsync(player, window);

            foreach (var id in ids)
            {
                await list.Push(id);

                var item = await GetItem(repository, cacheManager, id);
                if (item is not null)
                {
                    yield return item;
                }
            }
        }
    }

    public static async Task<bool> Destroy(this ItemInstance item, ICacheManager cacheManager)
    {
        var key = "item:" + item.Id;

        if (item.PlayerId != default)
        {
            var oldList = cacheManager.CreateList<Guid>($"items:{item.PlayerId}:{item.Window}");
            await oldList.Rem(1, item.Id);
        }

        return await cacheManager.Del(key) != 0;
    }

    public static Task Persist(this ItemInstance item, IItemRepository itemRepository)
    {
        return itemRepository.SaveItemAsync(item);
    }

    /// <summary>
    /// Sets the item position, window, and owner.
    /// Refresh the cache lists if needed, and persists the item
    /// </summary>
    /// <param name="item"></param>
    /// <param name="cacheManager"></param>
    /// <param name="owner">Owner the item is given to</param>
    /// <param name="window">Window the item is placed in</param>
    /// <param name="pos">Position of the item in the window</param>
    public static async Task Set(this ItemInstance item, ICacheManager cacheManager, uint owner, byte window, uint pos,
        IItemRepository itemRepository)
    {
        if (item.PlayerId != owner || item.Window != window)
        {
            if (item.PlayerId != default)
            {
                // Remove from last list
                var oldList = cacheManager.CreateList<Guid>($"items:{item.PlayerId}:{item.Window}");
                await oldList.Rem(1, item.Id);
            }

            if (owner != default)
            {
                var newList = cacheManager.CreateList<Guid>($"items:{owner}:{window}");
                await newList.Push(item.Id);
            }

            item.PlayerId = owner;
            item.Window = window;
        }

        item.Position = pos;
        await Persist(item, itemRepository);
    }
}
