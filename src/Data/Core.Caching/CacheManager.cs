using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.API;

namespace QuantumCore.Caching;

public sealed class CacheManager : ICacheManager
{
    public IRedisStore Shared { get; }
    public IRedisStore Server { get; }

    private readonly IRedisStore _defaultRedisStore;

    public CacheManager(ILogger<CacheManager> logger, IServiceProvider serviceProvider)
    {
        logger.LogInformation("Initialize Cache Manager");

        Shared = serviceProvider.GetRequiredKeyedService<IRedisStore>(CacheStoreType.SHARED);
        Server = serviceProvider.GetRequiredKeyedService<IRedisStore>(CacheStoreType.SERVER);

        _defaultRedisStore = Shared;
    }

    public IRedisListWrapper<T> CreateList<T>(string name) => _defaultRedisStore.CreateList<T>(name);
    public ValueTask<long> DelAsync(string key) => _defaultRedisStore.DelAsync(key);
    public ValueTask<string> SetAsync(string key, object item) => _defaultRedisStore.SetAsync(key, item);
    public ValueTask<T> GetAsync<T>(string key) => _defaultRedisStore.GetAsync<T>(key);
    public ValueTask<long> ExistsAsync(string key) => _defaultRedisStore.ExistsAsync(key);
    public ValueTask<long> ExpireAsync(string key, TimeSpan seconds) => _defaultRedisStore.ExpireAsync(key, seconds);
    public ValueTask<bool> PingAsync() => _defaultRedisStore.PingAsync();
    public ValueTask<long> PublishAsync(string key, object obj) => _defaultRedisStore.PublishAsync(key, obj);
    public IRedisSubscriber Subscribe() => _defaultRedisStore.Subscribe();
    public ValueTask<string[]> KeysAsync(string key) => _defaultRedisStore.KeysAsync(key);
    public ValueTask<long> PersistAsync(string key) => _defaultRedisStore.PersistAsync(key);
    public ValueTask<string> FlushAllAsync() => _defaultRedisStore.FlushAllAsync();
    public ValueTask DelAllAsync(string pattern) => _defaultRedisStore.DelAllAsync(pattern);
    public ValueTask<long> IncrAsync(string key) => _defaultRedisStore.IncrAsync(key);
}