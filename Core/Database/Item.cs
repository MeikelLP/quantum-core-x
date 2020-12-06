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
        public Guid PlayerId { get; set; }
        public uint ItemId { get; set; }
        public byte Window { get; set; }
        public uint Position { get; set; }
        public byte Count { get; set; }

        public static async Task<Item> GetItem(Guid id)
        {
            var redis = CacheManager.Redis;
            var key = "item:" + id;

            if (await redis.Exists(key) > 0)
            {
                Log.Debug($"Read item {id} from cache");
                return await redis.Get<Item>(key);
            }

            Log.Debug($"Load item {id} from database");
            using var db = DatabaseManager.GetGameDatabase();

            var item = db.Get<Item>(id);
            await redis.Set(key, item);
            return item;
        }
        
        public static async IAsyncEnumerable<Item> GetItems(Guid player, byte window)
        {
            var redis = CacheManager.Redis;
            var key = "items:" + player + ":window";

            var list = redis.CreateList<Guid>(key);
            
            // Check if the window list exists
            if (await redis.Exists(key) > 0)
            {
                Log.Debug($"Found items for player {player} in window {window} in cache");
                var itemIds = await list.Range(0, -1);

                foreach (var id in itemIds)
                {
                    yield return await GetItem(id);
                }
            }
            else
            {
                Log.Debug($"Query items for player {player} in window {window}");
                using var db = DatabaseManager.GetGameDatabase();
                var ids = await db.QueryAsync(
                    "SELECT Id FROM items WHERE PlayerId = @PlayerId AND Window = @Window",
                    new {PlayerId = player, Window = window});

                foreach (var row in ids)
                {
                    Guid itemId = row.Id;
                    await list.Push(itemId);

                    yield return await GetItem(itemId);
                }
            }
        }
    }
}