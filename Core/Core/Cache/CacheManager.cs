using System.Threading.Tasks;
using BeetleX.Redis;
using Dapper.Contrib.Extensions;
using QuantumCore.Database;
using Serilog;

namespace QuantumCore.Cache
{
    public static class CacheManager
    {
        public static RedisDB Redis { get; private set; }
        
        public static void Init(string host, int port = 6379)
        {
            Redis = new RedisDB {DataFormater = new JsonFormater()};
            Redis.Host.AddWriteHost(host, port);
        }

        public static async Task<T> Get<T>(object id) where T : class
        {
            var keyName = typeof(T).Name + ":" + id;

            if (await Redis.Exists(keyName) > 0)
            {
                // We have the object in cache! We're good to go
                Log.Debug($"Found {typeof(T).Name} with id {id} in cache");
                return await Redis.Get<T>(keyName);
            }

            Log.Debug($"Query {typeof(T).Name} with id {id} from the database");
            // We have to query the object from the database, cache it and return it
            using var db = DatabaseManager.GetGameDatabase();
            var obj = await db.GetAsync<T>(id);
            await Redis.Set(keyName, obj);
            
            return obj;
        }
    }
}