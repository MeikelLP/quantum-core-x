using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using QuantumCore.Cache;
using Serilog;

namespace QuantumCore.Database
{
    [System.ComponentModel.DataAnnotations.Schema.Table("items")]
    public class Item : BaseModel
    {
        public Guid PlayerId { get; private set; }
        public uint ItemId { get; set; }
        public byte Window { get; private set; }
        public uint Position { get; private set; }
        public byte Count { get; set; }

        public static async Task<Item> GetItem(IDatabaseManager databaseManager, Guid id)
        {
            var key = "item:" + id;

            if (await CacheManager.Instance.Exists(key) > 0)
            {
                Log.Debug($"Read item {id} from cache");
                return await CacheManager.Instance.Get<Item>(key);
            }

            Log.Debug($"Load item {id} from database");
            using var db = databaseManager.GetGameDatabase();

            var item = db.Get<Item>(id);
            await CacheManager.Instance.Set(key, item);
            return item;
        }
        
        public static async IAsyncEnumerable<Item> GetItems(IDatabaseManager databaseManager, Guid player, byte window)
        {
            var key = "items:" + player + ":" + window;

            var list = CacheManager.Instance.CreateList<Guid>(key);
            
            // Check if the window list exists
            if (await CacheManager.Instance.Exists(key) > 0)
            {
                Log.Debug($"Found items for player {player} in window {window} in cache");
                var itemIds = await list.Range(0, -1);

                foreach (var id in itemIds)
                {
                    yield return await GetItem(databaseManager, id);
                }
            }
            else
            {
                Log.Debug($"Query items for player {player} in window {window} from database");
                using var db = databaseManager.GetGameDatabase();
                var ids = await db.QueryAsync(
                    "SELECT Id FROM items WHERE PlayerId = @PlayerId AND `Window` = @Window",
                    new {PlayerId = player, Window = window});

                foreach (var row in ids)
                {
                    Guid itemId = row.Id;
                    await list.Push(itemId);

                    yield return await GetItem(databaseManager, itemId);
                }
            }
        }

        public async Task<bool> Destroy()
        {
            var key = "item:" + Id;

            if (PlayerId != Guid.Empty)
            {
                var oldList = CacheManager.Instance.CreateList<Guid>($"items:{PlayerId}:{Window}");
                await oldList.Rem(1, Id);
            }

            return await CacheManager.Instance.Del(key) != 0;
        }

        public async Task Persist()
        {
            var key = "item:" + Id;

            await CacheManager.Instance.Set(key, this);
        }

        /// <summary>
        /// Sets the item position, window, and owner.
        /// Refresh the cache lists if needed, and persists the item
        /// </summary>
        /// <param name="owner">Owner the item is given to</param>
        /// <param name="window">Window the item is placed in</param>
        /// <param name="pos">Position of the item in the window</param>
        public async Task Set(Guid owner, byte window, uint pos)
        {
            if (PlayerId != owner || Window != window)
            {
                if (PlayerId != Guid.Empty)
                {
                    // Remove from last list
                    var oldList = CacheManager.Instance.CreateList<Guid>($"items:{PlayerId}:{Window}");
                    await oldList.Rem(1, Id);
                }

                if (owner != Guid.Empty)
                {
                    var newList = CacheManager.Instance.CreateList<Guid>($"items:{owner}:{window}");
                    await newList.Push(Id);
                }

                PlayerId = owner;
                Window = window;
            }

            Position = pos;
            await Persist();
        }
    }
}