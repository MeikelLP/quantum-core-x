using System.Data;
using BeetleX.Redis;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuantumCore.Caching
{
    public class CacheManager : ICacheManager
    {
        private readonly IDbConnection _db;
        private readonly ILogger<CacheManager> _logger;
        private readonly RedisDB _redis;

        public CacheManager(IDbConnection db, ILogger<CacheManager> logger, IOptions<CacheOptions> options)
        {
            _db = db;
            _logger = logger;
            _redis = new RedisDB { DataFormater = new JsonFormater() };
            _logger.LogInformation("Initialize Cache Manager");
            _redis.Host.AddWriteHost(options.Value.Host, options.Value.Port);
        }

        public async ValueTask<T> GetOrCreate<T>(object id) where T : class
        {
            var keyName = $"{typeof(T).Name}:{id}";

            if (await _redis.Exists(keyName) > 0)
            {
                // We have the object in cache! We're good to go
                _logger.LogDebug("Found {Type} with id {Id} in cache", typeof(T).Name, id);
                return await _redis.Get<T>(keyName);
            }

            _logger.LogDebug("Query {Type} with id {Id} from the database", typeof(T).Name, id);
            // We have to query the object from the database, cache it and return it
            var obj = await _db.GetAsync<T>(id);
            await _redis.Set(keyName, obj);
            
            return obj;
        }

        public IRedisListWrapper<T> CreateList<T>(string name) => new RedisListWrapper<T>(_redis.CreateList<T>(name));
        public ValueTask<long> Del(string key) => _redis.Del(key);
        public ValueTask<string> Set(string key, object item) => _redis.Set(key, item);
        public ValueTask<T> Get<T>(string key) => _redis.Get<T>(key);
        public ValueTask<long> Exists(string key) => _redis.Exists(key);
        public ValueTask<long> Expire(string key, int seconds) => _redis.Expire(key, seconds);
        public ValueTask<bool> Ping() => _redis.Ping();
        public ValueTask<long> Publish(string key, object obj) => _redis.Publish(key, obj);
        public IRedisSubscriber Subscribe() => new RedisSubscriber(_redis.Subscribe());
        public ValueTask<string[]> Keys(string key) => _redis.Keys(key);
        public ValueTask<long> Persist(string key) => _redis.Persist(key);
    }
}