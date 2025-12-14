using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types.Items;
using QuantumCore.API.Game.Types.Players;
using QuantumCore.Caching;
using QuantumCore.Game.Persistence;
using static QuantumCore.Game.Extensions.ItemConstants;

namespace QuantumCore.Game.Extensions;

public static class ItemExtensions
{
    public static uint GetMinWeaponBaseDamage(this ItemData item)
    {
        return (uint)item.Values[3];
    }

    public static uint GetMaxWeaponBaseDamage(this ItemData item)
    {
        return (uint)item.Values[4];
    }

    public static uint GetMinMagicWeaponBaseDamage(this ItemData item)
    {
        return (uint)item.Values[1];
    }

    public static uint GetMaxMagicWeaponBaseDamage(this ItemData item)
    {
        return (uint)item.Values[2];
    }

    public static int GetApplyValue(this ItemData item, EApplyType type)
    {
        var apply = item.Applies.FirstOrDefault(x => (EApplyType)x.Type == type);

        return (int)(apply?.Value ?? 0);
    }

    /// <summary>
    /// Weapon damage added additionally to the base damage
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static uint GetAdditionalWeaponDamage(this ItemData item)
    {
        return (uint)item.Values[5];
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

    public static bool IsType(this ItemData item, EItemType type)
    {
        return (EItemType)item.Type == type;
    }
    
    public static bool IsSubtype(this ItemData item, EItemSubtype subtype)
    {
        return (EItemSubtype)item.Subtype == subtype;
    }

    public static uint GetHairPartOffsetForClient(this ItemInstance? itemInstance, EPlayerClass playerClass)
    {
        if (itemInstance is null)
        {
            return 0;
        }
       
        var itemId = itemInstance.ItemId;
        if (itemId < HairPartIdOffsets.WarOffsetBase)
        {
            return 0;
        }
        
        switch (playerClass)
        {
            case EPlayerClass.Warrior:
                return itemId - HairPartIdOffsets.WarOffsetBase; // 73001 - 72000 = 1001 start hair number from
            case EPlayerClass.Ninja:
                return itemId - HairPartIdOffsets.NinjaOffsetBase;
            case EPlayerClass.Sura:
                return itemId - HairPartIdOffsets.SuraOffsetBase;
            case EPlayerClass.Shaman:
                return itemId - HairPartIdOffsets.ShamanOffsetBase;
            default:
                throw new NotImplementedException();
        }
    }

    public static EquipmentSlot? GetWearSlot(this IItemManager itemManager, uint itemId)
    {
        var proto = itemManager.GetItem(itemId);
        if (proto == null)
        {
            return null;
        }

        return proto.GetWearSlot();
    }

    public static EquipmentSlot? GetWearSlot(this ItemData proto)
    {
        if (proto.IsType(EItemType.Costume))
        {
            if (proto.IsSubtype(EItemSubtype.CostumeBody))
            {
                return EquipmentSlot.Costume;
            }
            if (proto.IsSubtype(EItemSubtype.CostumeHair))
            {
                return EquipmentSlot.Hair;
            }
        }

        return ((EWearFlags)proto.WearFlags).GetWearSlot();
    }

    private static EquipmentSlot? GetWearSlot(this EWearFlags wearFlags)
    {
        if (wearFlags.HasFlag(EWearFlags.Head))
        {
            return EquipmentSlot.Head;
        }

        if (wearFlags.HasFlag(EWearFlags.Shoes))
        {
            return EquipmentSlot.Shoes;
        }

        if (wearFlags.HasFlag(EWearFlags.Bracelet))
        {
            return EquipmentSlot.Bracelet;
        }

        if (wearFlags.HasFlag(EWearFlags.Weapon))
        {
            return EquipmentSlot.Weapon;
        }

        if (wearFlags.HasFlag(EWearFlags.Necklace))
        {
            return EquipmentSlot.Necklace;
        }

        if (wearFlags.HasFlag(EWearFlags.Earrings))
        {
            return EquipmentSlot.Earring;
        }

        if (wearFlags.HasFlag(EWearFlags.Body))
        {
            return EquipmentSlot.Body;
        }

        if (wearFlags.HasFlag(EWearFlags.Shield))
        {
            return EquipmentSlot.Shield;
        }

        throw new NotImplementedException($"No equipment slot for wear flags: {wearFlags}");
    }

    public static async Task<ItemInstance?> GetItem(this IItemRepository repository, ICacheManager cacheManager,
        Guid id)
    {
        var key = "item:" + id;

        if (await cacheManager.Server.Exists(key) > 0)
        {
            return await cacheManager.Server.Get<ItemInstance>(key);
        }

        var item = await repository.GetItemAsync(id);
        await cacheManager.Server.Set(key, item);
        return item;
    }

    public static async Task DeletePlayerItemAsync(this IItemRepository repository, ICacheManager cacheManager,
        uint playerId, uint itemId)
    {
        var key = $"item:{itemId}";

        await cacheManager.Del(key);

        await repository.DeletePlayerItemAsync(playerId, itemId);
    }

    public static async IAsyncEnumerable<ItemInstance> GetItems(this IItemRepository repository,
        ICacheManager cacheManager, uint player, WindowType window)
    {
        var key = "items:" + player + ":" + (byte)window;

        var list = cacheManager.Server.CreateList<Guid>(key);

        // Check if the window list exists
        if (await cacheManager.Server.Exists(key) > 0)
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
            var oldList = cacheManager.Server.CreateList<Guid>($"items:{item.PlayerId}:{item.Window}");
            await oldList.Rem(1, item.Id);
        }

        return await cacheManager.Server.Del(key) != 0;
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
    public static async Task Set(this ItemInstance item, ICacheManager cacheManager, uint owner, WindowType window,
        uint pos, IItemRepository itemRepository)
    {
        var isPlayerDifferent = item.PlayerId != owner;
        var isWindowDifferent = item.Window != window;

        item.PlayerId = owner;
        item.Window = window;
        item.Position = pos;
        await Persist(item, itemRepository);

        if (isPlayerDifferent || isWindowDifferent)
        {
            if (item.PlayerId != default)
            {
                // Remove from last list
                var oldList = cacheManager.Server.CreateList<Guid>($"items:{item.PlayerId}:{item.Window}");
                await oldList.Rem(1, item.Id);
            }

            if (owner != default)
            {
                var newList = cacheManager.Server.CreateList<Guid>($"items:{owner}:{window}");
                await newList.Push(item.Id);
            }
        }
    }
}
