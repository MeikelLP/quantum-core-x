using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types.Items;
using QuantumCore.API.Game.Types.Players;
using static QuantumCore.API.ItemConstants;

namespace QuantumCore.API.Extensions;

public static class ItemExtensions
{
    extension(ItemData item)
    {
        public uint MinWeaponBaseDamage => (uint)item.Values[3];
        public uint MaxWeaponBaseDamage => (uint)item.Values[4];
        public uint MinMagicWeaponBaseDamage => (uint)item.Values[1];
        public uint MaxMagicWeaponBaseDamage => (uint)item.Values[2];

        /// <summary>
        /// Weapon damage added additionally to the base damage
        /// </summary>
        /// <returns></returns>
        public uint AdditionalWeaponDamage => (uint)item.Values[5];

        public uint MinWeaponDamage => item.MinWeaponBaseDamage + item.AdditionalWeaponDamage;
        public uint MaxWeaponDamage => item.MaxWeaponBaseDamage + item.AdditionalWeaponDamage;
        public uint MinMagicWeaponDamage => item.MinMagicWeaponBaseDamage + item.AdditionalWeaponDamage;
        public uint MaxMagicWeaponDamage => item.MaxMagicWeaponBaseDamage + item.AdditionalWeaponDamage;

        public int GetApplyValue(EApplyType type)
        {
            var apply = item.Applies.FirstOrDefault(x => (EApplyType)x.Type == type);

            return (int)(apply?.Value ?? 0);
        }

        public bool IsType(EItemType type)
        {
            return (EItemType)item.Type == type;
        }

        public bool IsSubtype(EItemSubtype subtype)
        {
            return (EItemSubtype)item.Subtype == subtype;
        }

        public EquipmentSlot? GetWearSlot()
        {
            if (item.IsType(EItemType.COSTUME))
            {
                if (item.IsSubtype(EItemSubtype.COSTUME_BODY))
                {
                    return EquipmentSlot.COSTUME;
                }

                if (item.IsSubtype(EItemSubtype.COSTUME_HAIR))
                {
                    return EquipmentSlot.HAIR;
                }
            }

            return ((EWearFlags)item.WearFlags).GetWearSlot();
        }
    }

    extension(ItemInstance? itemInstance)
    {
        public uint GetHairPartOffsetForClient(EPlayerClass playerClass)
        {
            if (itemInstance is null)
            {
                return 0;
            }

            var itemId = itemInstance.ItemId;
            if (itemId < HairPartIdOffsets.WAR_OFFSET_BASE)
            {
                return 0;
            }

            switch (playerClass)
            {
                case EPlayerClass.WARRIOR:
                    return itemId - HairPartIdOffsets.WAR_OFFSET_BASE; // 73001 - 72000 = 1001 start hair number from
                case EPlayerClass.NINJA:
                    return itemId - HairPartIdOffsets.NINJA_OFFSET_BASE;
                case EPlayerClass.SURA:
                    return itemId - HairPartIdOffsets.SURA_OFFSET_BASE;
                case EPlayerClass.SHAMAN:
                    return itemId - HairPartIdOffsets.SHAMAN_OFFSET_BASE;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    extension(IItemManager itemManager)
    {
        public EquipmentSlot? GetWearSlot(uint itemId)
        {
            var proto = itemManager.GetItem(itemId);
            if (proto is null)
            {
                return null;
            }

            return proto.GetWearSlot();
        }
    }

    private static EquipmentSlot? GetWearSlot(this EWearFlags wearFlags)
    {
        if (wearFlags.HasFlag(EWearFlags.HEAD))
        {
            return EquipmentSlot.HEAD;
        }

        if (wearFlags.HasFlag(EWearFlags.SHOES))
        {
            return EquipmentSlot.SHOES;
        }

        if (wearFlags.HasFlag(EWearFlags.BRACELET))
        {
            return EquipmentSlot.BRACELET;
        }

        if (wearFlags.HasFlag(EWearFlags.WEAPON))
        {
            return EquipmentSlot.WEAPON;
        }

        if (wearFlags.HasFlag(EWearFlags.NECKLACE))
        {
            return EquipmentSlot.NECKLACE;
        }

        if (wearFlags.HasFlag(EWearFlags.EARRINGS))
        {
            return EquipmentSlot.EARRING;
        }

        if (wearFlags.HasFlag(EWearFlags.BODY))
        {
            return EquipmentSlot.BODY;
        }

        if (wearFlags.HasFlag(EWearFlags.SHIELD))
        {
            return EquipmentSlot.SHIELD;
        }

        throw new NotImplementedException($"No equipment slot for wear flags: {wearFlags}");
    }

    extension(IItemRepository repository)
    {
        public async Task<ItemInstance?> GetItemAsync(ICacheManager cacheManager,
            Guid id)
        {
            ArgumentNullException.ThrowIfNull(cacheManager);
            var key = "item:" + id;

            if (await cacheManager.Server.ExistsAsync(key) > 0)
            {
                return await cacheManager.Server.GetAsync<ItemInstance>(key);
            }

            var item = await repository.GetItemAsync(id);
            if (item is not null)
            {
                await cacheManager.Server.SetAsync(key, item);
            }

            return item;
        }

        public async Task DeletePlayerItemAsync(ICacheManager cacheManager,
            uint playerId, uint itemId)
        {
            ArgumentNullException.ThrowIfNull(cacheManager);
            var key = $"item:{itemId}";

            await cacheManager.DelAsync(key);

            await repository.DeletePlayerItemAsync(playerId, itemId);
        }

        public async IAsyncEnumerable<ItemInstance> GetItemsAsync(ICacheManager cacheManager, uint player,
            WindowType window)
        {
            ArgumentNullException.ThrowIfNull(cacheManager);
            var key = "items:" + player + ":" + (byte)window;

            var list = cacheManager.Server.CreateList<Guid>(key);

            // Check if the window list exists
            if (await cacheManager.Server.ExistsAsync(key) > 0)
            {
                var itemIds = await list.RangeAsync(0, -1);

                foreach (var id in itemIds)
                {
                    var item = await GetItemAsync(repository, cacheManager, id);
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
                    await list.PushAsync(id);

                    var item = await GetItemAsync(repository, cacheManager, id);
                    if (item is not null)
                    {
                        yield return item;
                    }
                }
            }
        }
    }

    extension(ItemInstance item)
    {
        public async Task<bool> DestroyAsync(ICacheManager cacheManager)
        {
            ArgumentNullException.ThrowIfNull(cacheManager);
            var key = "item:" + item.Id;

            if (item.PlayerId != 0)
            {
                var oldList = cacheManager.Server.CreateList<Guid>($"items:{item.PlayerId}:{item.Window}");
                await oldList.RemAsync(1, item.Id);
            }

            return await cacheManager.Server.DelAsync(key) != 0;
        }

        public Task PersistAsync(IItemRepository itemRepository)
        {
            ArgumentNullException.ThrowIfNull(itemRepository);
            return itemRepository.SaveItemAsync(item);
        }

        /// <summary>
        /// Sets the item position, window, and owner.
        /// Refresh the cache lists if needed, and persists the item
        /// </summary>
        public async Task SetAsync(ICacheManager cacheManager, uint owner, WindowType window,
            uint pos, IItemRepository itemRepository)
        {
            ArgumentNullException.ThrowIfNull(cacheManager);
            var isPlayerDifferent = item.PlayerId != owner;
            var isWindowDifferent = item.Window != window;

            item.PlayerId = owner;
            item.Window = window;
            item.Position = pos;
            await item.PersistAsync(itemRepository);

            if (isPlayerDifferent || isWindowDifferent)
            {
                if (item.PlayerId != 0)
                {
                    // Remove from last list
                    var oldList = cacheManager.Server.CreateList<Guid>($"items:{item.PlayerId}:{item.Window}");
                    await oldList.RemAsync(1, item.Id);
                }

                if (owner != 0)
                {
                    var newList = cacheManager.Server.CreateList<Guid>($"items:{owner}:{window}");
                    await newList.PushAsync(item.Id);
                }
            }
        }
    }
}