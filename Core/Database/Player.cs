using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using QuantumCore.Caching;

namespace QuantumCore.Database
{
    [System.ComponentModel.DataAnnotations.Schema.Table("players")]
    public class Player : BaseModel, ICache
    {
        public Guid AccountId { get; set; }
        public string Name { get; set; }
        public byte PlayerClass { get; set; }
        public byte SkillGroup { get; set; }
        public uint PlayTime { get; set; }
        public byte Level { get; set; } = 1;
        public uint Experience { get; set; }
        public uint Gold { get; set; }
        public byte St { get; set; }
        public byte Ht { get; set; }
        public byte Dx { get; set; }
        public byte Iq { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public long Health { get; set; }
        public long Mana { get; set; }
        public long Stamina { get; set; }
        public uint BodyPart { get; set; }
        public uint HairPart { get; set; }
        public uint GivenStatusPoints { get; set; }
        public uint AvailableStatusPoints { get; set; }

        public static async Task<Player> GetPlayer(IDbConnection db, ICacheManager cacheManager, Guid account, byte slot)
        {
            var key = "players:" + account;
            
            var list = cacheManager.CreateList<Guid>(key);
            if (await cacheManager.Exists(key) <= 0)
            {
                var i = 0;
                await foreach (var player in GetPlayers(db, cacheManager, account))
                {
                    if (i == slot) return player;
                    i++;
                }

                return null;
            }

            var playerId = await list.Index(slot);
            return await GetPlayer(db, cacheManager, playerId);
        }

        public static async Task<Player> GetPlayer(IDbConnection db, ICacheManager cacheManager, Guid playerId)
        {
            var playerKey = "player:" + playerId;
            if (await cacheManager.Exists(playerKey) > 0)
            {
                return await cacheManager.Get<Player>(playerKey);
            }
            else
            {
                var player = db.Get<Player>(playerId);
                //var player = await SqlMapperExtensions.Get<Player>(db, playerId);
                await cacheManager.Set(playerKey, player);
                return player;
            }
        }
        
        public static async IAsyncEnumerable<Player> GetPlayers(IDbConnection db, ICacheManager cacheManager, Guid account)
        {
            var key = "players:" + account;

            var list = cacheManager.CreateList<Guid>(key);
            
            // Check if we have players cached
            if (await cacheManager.Exists(key) > 0)
            {
                // We have the characters cached
                var cachedIds = await list.Range(0, -1);

                foreach (var id in cachedIds)
                {
                    yield return await cacheManager.Get<Player>("player:" + id);    
                }
            }
            else
            {
                var ids = await db.QueryAsync("SELECT Id FROM players WHERE AccountId = @AccountId",
                    new {AccountId = account});

                // todo: is it ever possible that we have a player cached but not the players list? 
                //  if this is not the case we can make this part short and faster
                foreach (var row in ids)
                {
                    Guid playerId = row.Id;
                    await list.Push(playerId);

                    yield return await GetPlayer(db, cacheManager, playerId);
                }
            }
        }
    }
}