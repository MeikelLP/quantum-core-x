using BeetleX.Redis;
using Microsoft.Extensions.Logging;
using EnumsNET;

namespace QuantumCore.Caching;

public class RedisStore: IRedisStore
{
    private readonly RedisDB _redis;

    public RedisStore(CacheStoreType db, ILogger logger, CacheOptions options)
    {
        _redis = new RedisDB ((int)db, new JsonFormater());
        logger.LogInformation("Initialize {store} Cache Store", db.AsString(EnumFormat.EnumMemberValue));
        _redis.Host.AddWriteHost(options.Host, options.Port);
    }
    
    public IRedisListWrapper<T> CreateList<T>(string name) => new RedisListWrapper<T>(_redis.CreateList<T>(name));
    public ValueTask<long> Del(string key) => _redis.Del(key);
    public ValueTask<string> Set(string key, object item) => _redis.Set(key, item);
    public ValueTask<T> Get<T>(string key) => _redis.Get<T>(key);
    public ValueTask<long> Exists(string key) => _redis.Exists(key);
    public ValueTask<long> Expire(string key, TimeSpan seconds) => _redis.Expire(key, (int)seconds.TotalSeconds);
    public ValueTask<bool> Ping() => _redis.Ping();
    public ValueTask<long> Publish(string key, object obj) => _redis.Publish(key, obj);
    public IRedisSubscriber Subscribe() => new RedisSubscriber(_redis.Subscribe());
    public ValueTask<string[]> Keys(string key) => _redis.Keys(key);
    public ValueTask<long> Persist(string key) => _redis.Persist(key);
    public ValueTask<string> FlushAll() => _redis.Flushall();
    

    public async void DelAllAsync(string pattern)
    {
        var keys = await _redis.Keys(pattern);
        
        foreach (var key in keys)
        {
            await _redis.Del(key);
        }
    }

    public ValueTask<long> Incr(string key) => _redis.Incr(key);
}