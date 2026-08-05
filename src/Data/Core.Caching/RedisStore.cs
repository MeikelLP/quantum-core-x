using BeetleX.Redis;
using EnumsNET;
using Microsoft.Extensions.Logging;
using QuantumCore.API;

namespace QuantumCore.Caching;

public sealed class RedisStore : IRedisStore, IDisposable
{
    private readonly RedisDB _redis;

    public RedisStore(CacheStoreType db, ILogger logger, CacheOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _redis = new RedisDB((int)db, new JsonFormater());
        logger.LogInformation("Initialize {Store} Cache Store", db.AsString(EnumFormat.EnumMemberValue));
        var host = _redis.Host.AddWriteHost(options.Host, options.Port);
        host.Password = options.Password;
    }

    public IRedisListWrapper<T> CreateList<T>(string name) => new RedisListWrapper<T>(_redis.CreateList<T>(name));
    public ValueTask<long> DelAsync(string key) => _redis.Del(key);
    public ValueTask<string> SetAsync(string key, object item) => _redis.Set(key, item);
    public ValueTask<T> GetAsync<T>(string key) => _redis.Get<T>(key);
    public ValueTask<long> ExistsAsync(string key) => _redis.Exists(key);
    public ValueTask<long> ExpireAsync(string key, TimeSpan seconds) => _redis.Expire(key, (int)seconds.TotalSeconds);
    public ValueTask<bool> PingAsync() => _redis.Ping();
    public ValueTask<long> PublishAsync(string key, object obj) => _redis.Publish(key, obj);
    public IRedisSubscriber Subscribe() => new RedisSubscriber(_redis.Subscribe());
    public ValueTask<string[]> KeysAsync(string key) => _redis.Keys(key);
    public ValueTask<long> PersistAsync(string key) => _redis.Persist(key);
    public ValueTask<string> FlushAllAsync() => _redis.Flushall();


    public async ValueTask DelAllAsync(string pattern)
    {
        var keys = await _redis.Keys(pattern);

        foreach (var key in keys)
        {
            await _redis.Del(key);
        }
    }

    public ValueTask<long> IncrAsync(string key) => _redis.Incr(key);

    public void Dispose()
    {
        _redis.Dispose();
    }
}