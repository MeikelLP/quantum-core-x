using BeetleX.Redis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuantumCore.Core.Cache
{
    public class CacheManager : ICacheManager
    {
        private readonly ILogger<CacheManager> _logger;
        private readonly RedisDB _redis;

        public CacheManager(ILogger<CacheManager> logger, IOptions<CacheOptions> options)
        {
            _logger = logger;
            _redis = new RedisDB { DataFormater = new JsonFormater() };
            _logger.LogInformation("Initialize Cache Manager");
            _redis.Host.AddWriteHost(options.Value.Host, options.Value.Port);
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
