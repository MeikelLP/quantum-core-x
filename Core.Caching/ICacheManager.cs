namespace QuantumCore.Caching;

public interface ICacheManager
{
    IRedisListWrapper<T> CreateList<T>(string name);
    ValueTask<long> Del(string key);
    ValueTask<string> Set(string key, object item);
    ValueTask<T> Get<T>(string key);
    ValueTask<T> GetOrCreate<T>(object id) where T : class;
    ValueTask<long> Exists(string key);
    ValueTask<long> Expire(string key, int seconds);
    ValueTask<bool> Ping();
    ValueTask<long> Publish(string key, object obj);
    IRedisSubscriber Subscribe();
    ValueTask<string[]> Keys(string key);
    ValueTask<long> Persist(string key);
}