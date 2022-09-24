using System.Collections.Generic;
using System.Threading.Tasks;
using BeetleX.Redis;
using Dapper.Contrib.Extensions;
using QuantumCore.Database;
using Serilog;

namespace QuantumCore.Cache
{
    public class CacheManager : ICacheManager
    {
        private readonly IDatabaseManager _databaseManager;

        private CacheManager(IDatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }
        
        public static ICacheManager Instance { get; private set; }
        private RedisDB Redis;
        
        public static void Init(IDatabaseManager databaseManager, string host, int port = 6379)
        {
            Log.Information("Initialize Cache Manager");
            var instance = new CacheManager (databaseManager) {
                Redis = new RedisDB {
                    DataFormater = new JsonFormater()
                }
            };
            instance.Redis.Host.AddWriteHost(host, port);
            Instance = instance;
        }

        public async ValueTask<T> GetOrCreate<T>(object id) where T : class
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
            using var db = _databaseManager.GetGameDatabase();
            var obj = await db.GetAsync<T>(id);
            await Redis.Set(keyName, obj);
            
            return obj;
        }

        public RedisList<T> CreateList<T>(string name) => Redis.CreateList<T>(name);
        public ValueTask<long> Del(string key) => Redis.Del(key);
        public ValueTask<string> Set(string key, object item) => Redis.Set(key, item);
        public ValueTask<T> Get<T>(string key) => Redis.Get<T>(key);
        public ValueTask<long> Exists(string key) => Redis.Exists(key);
        public ValueTask<long> Expire(string key, int seconds) => Redis.Expire(key, seconds);
        public ValueTask<bool> Ping() => Redis.Ping();
        public ValueTask<long> Publish(string key, object obj) => Redis.Publish(key, obj);
        public Subscriber Subscribe() => Redis.Subscribe();
        public ValueTask<string[]> Keys(string key) => Redis.Keys(key);
        public ValueTask<long> Persist(string key) => Redis.Persist(key);
    }
}