namespace QuantumCore.API;

public interface IRedisStore
{
    IRedisListWrapper<T> CreateList<T>(string name);
    ValueTask<long> DelAsync(string key);
    ValueTask<string> SetAsync(string key, object item);
    ValueTask<T> GetAsync<T>(string key);
    ValueTask<long> ExistsAsync(string key);
    ValueTask<long> ExpireAsync(string key, TimeSpan seconds);
    ValueTask<bool> PingAsync();
    ValueTask<long> PublishAsync(string key, object obj);
    IRedisSubscriber Subscribe();
    ValueTask<string[]> KeysAsync(string key);
    ValueTask<long> PersistAsync(string key);
    ValueTask<string> FlushAllAsync();
    ValueTask DelAllAsync(string pattern);
    ValueTask<long> IncrAsync(string key);
}