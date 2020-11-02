using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using Dapper;
using QuantumCore.Cache;
using Serilog;

namespace QuantumCore.Database
{
    [Table("players")]
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

        public async IAsyncEnumerable<Player> GetPlayers(Guid account)
        {
            var key = "players:" + account;
            
            // Check if we have players cached
            var redis = CacheManager.Redis;
            if (await redis.Exists(key) > 0)
            {
                Log.Debug($"Found players for account {account} in cache");
                // We have the characters cached
                var list = redis.CreateList<Guid>(key);
                var cachedIds = await list.Range(0, -1);

                foreach (var id in cachedIds)
                {
                    yield return await redis.Get<Player>("player:" + id);    
                }
            }
            
            Log.Debug($"Query players for account {account} from the database");
            using var db = DatabaseManager.GetGameDatabase();
            var ids = await db.QueryAsync("SELECT Id FROM players WHERE AccountId = @AccountId", new {AccountId = account});
            
        }
    }
}