using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuantumCore.Caching;

public class CacheManager : ICacheManager
{
    private readonly ILogger<CacheManager> _logger;

    public IRedisStore Shared { get; }
    public IRedisStore Server { get; }
    
    private IRedisStore _defaultRedisStore;

    public CacheManager(ILogger<CacheManager> logger, IOptions<CacheOptions> options)
    {
        _logger = logger;
        _logger.LogInformation("Initialize Cache Manager");

        Shared = new RedisStore(CacheStoreType.Shared, logger, options.Value);
        Server = new RedisStore(CacheStoreType.Server, logger, options.Value);

        _defaultRedisStore = Shared;
    }
    public IRedisListWrapper<T> CreateList<T>(string name) => _defaultRedisStore.CreateList<T>(name);
    public ValueTask<long> Del(string key) => _defaultRedisStore.Del(key);
    public ValueTask<string> Set(string key, object item) => _defaultRedisStore.Set(key, item);
    public ValueTask<T> Get<T>(string key) => _defaultRedisStore.Get<T>(key);
    public ValueTask<long> Exists(string key) => _defaultRedisStore.Exists(key);
    public ValueTask<long> Expire(string key, int seconds) => _defaultRedisStore.Expire(key, seconds);
    public ValueTask<bool> Ping() => _defaultRedisStore.Ping();
    public ValueTask<long> Publish(string key, object obj) => _defaultRedisStore.Publish(key, obj);
    public IRedisSubscriber Subscribe() => _defaultRedisStore.Subscribe();
    public ValueTask<string[]> Keys(string key) => _defaultRedisStore.Keys(key);
    public ValueTask<long> Persist(string key) => _defaultRedisStore.Persist(key);
    public ValueTask<string> FlushAll() => _defaultRedisStore.FlushAll();
    public void DelAllAsync(string pattern) => _defaultRedisStore.DelAllAsync(pattern);
    public ValueTask<long> Incr(string key) => _defaultRedisStore.Incr(key);
}
