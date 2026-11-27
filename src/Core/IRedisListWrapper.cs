namespace QuantumCore.API;

public interface IRedisListWrapper
{
    ValueTask<long> LenAsync();
}

public interface IRedisListWrapper<T> : IRedisListWrapper
{
    ValueTask<T> IndexAsync(int slot);
    ValueTask<T[]> RangeAsync(int start, int stop);
    ValueTask<long> PushAsync(params T[] arr);
    ValueTask<long> RemAsync(int count, T obj);
}