using BeetleX.Redis;
using QuantumCore.API;

namespace QuantumCore.Caching;

/// <summary>
/// Wrapper for the <see cref="RedisList{T}"/>. As id does not implement any interfaces we need to wrap it in order
/// to mock it in our tests
/// </summary>
/// <typeparam name="T"></typeparam>
public class RedisListWrapper<T> : IRedisListWrapper<T>
{
    private readonly RedisList<T> _list;

    public RedisListWrapper(RedisList<T> list)
    {
        _list = list;
    }

    public ValueTask<T> IndexAsync(int slot) => _list.Index(slot);

    public ValueTask<T[]> RangeAsync(int start, int stop) => _list.Range(start, stop);

    public ValueTask<long> PushAsync(params T[] arr) => _list.Push(arr);

    public ValueTask<long> RemAsync(int count, T obj) => _list.Rem(count, obj);
    public ValueTask<long> LenAsync() => _list.Len();
}