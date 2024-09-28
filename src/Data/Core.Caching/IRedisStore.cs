namespace QuantumCore.Caching;

public interface IRedisStore
{
    IRedisListWrapper<T> CreateList<T>(string name);
    ValueTask<long> Del(string key);
    ValueTask<string> Set(string key, object item);
    ValueTask<T> Get<T>(string key);
    ValueTask<long> Exists(string key);
    ValueTask<long> Expire(string key, TimeSpan seconds);
    ValueTask<bool> Ping();
    ValueTask<long> Publish(string key, object obj);
    IRedisSubscriber Subscribe();
    ValueTask<string[]> Keys(string key);
    ValueTask<long> Persist(string key);
    ValueTask<string> FlushAll();
    void DelAllAsync(string pattern);
    ValueTask<long> Incr(string key);
}