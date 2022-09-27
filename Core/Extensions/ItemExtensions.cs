using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using QuantumCore.API.Core.Models;
using QuantumCore.Core.Cache;
using QuantumCore.Database;
using Serilog;

namespace QuantumCore.Extensions;

public static class ItemExtensions
{
    public static async Task<ItemInstance> GetItem(this IDatabaseManager databaseManager, ICacheManager cacheManager, Guid id)
    {
        var key = "item:" + id;

        if (await cacheManager.Exists(key) > 0)
        {
            Log.Debug($"Read item {id} from cache");
            return await cacheManager.Get<ItemInstance>(key);
        }

        Log.Debug($"Load item {id} from database");
        using var db = databaseManager.GetGameDatabase();

        var item = db.Get<ItemInstance>(id);
        await cacheManager.Set(key, item);
        return item;
    }

    public static async IAsyncEnumerable<ItemInstance> GetItems(this IDatabaseManager databaseManager, ICacheManager cacheManager, Guid player,
        byte window)
    {
        var key = "items:" + player + ":" + window;

        var list = cacheManager.CreateList<Guid>(key);

        // Check if the window list exists
        if (await cacheManager.Exists(key) > 0)
        {
            Log.Debug($"Found items for player {player} in window {window} in cache");
            var itemIds = await list.Range(0, -1);

            foreach (var id in itemIds)
            {
                yield return await GetItem(databaseManager, cacheManager, id);
            }
        }
        else
        {
            Log.Debug($"Query items for player {player} in window {window} from database");
            using var db = databaseManager.GetGameDatabase();
            var ids = await db.QueryAsync<Guid>(
                "SELECT Id FROM items WHERE PlayerId = @PlayerId AND `Window` = @Window",
                new { PlayerId = player, Window = window });

            foreach (var id in ids)
            {
                await list.Push(id);

                yield return await GetItem(databaseManager, cacheManager, id);
            }
        }
    }

    public static async Task<bool> Destroy(this ItemInstance item, ICacheManager cacheManager)
    {
        var key = "item:" + item.Id;

        if (item.PlayerId != Guid.Empty)
        {
            var oldList = cacheManager.CreateList<Guid>($"items:{item.PlayerId}:{item.Window}");
            await oldList.Rem(1, item.Id);
        }

        return await cacheManager.Del(key) != 0;
    }

    public static async Task Persist(this ItemInstance item, ICacheManager cacheManager)
    {
        var key = "item:" + item.Id;

        await cacheManager.Set(key, item);
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
    public static async Task Set(this ItemInstance item, ICacheManager cacheManager, Guid owner, byte window, uint pos)
    {
        if (item.PlayerId != owner || item.Window != window)
        {
            if (item.PlayerId != Guid.Empty)
            {
                // Remove from last list
                var oldList = cacheManager.CreateList<Guid>($"items:{item.PlayerId}:{item.Window}");
                await oldList.Rem(1, item.Id);
            }

            if (owner != Guid.Empty)
            {
                var newList = cacheManager.CreateList<Guid>($"items:{owner}:{window}");
                await newList.Push(item.Id);
            }

            item.PlayerId = owner;
            item.Window = window;
        }

        item.Position = pos;
        await Persist(item, cacheManager);
    }
}