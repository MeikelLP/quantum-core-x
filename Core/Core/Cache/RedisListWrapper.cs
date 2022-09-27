using System.Threading.Tasks;
using BeetleX.Redis;

namespace QuantumCore.Core.Cache;

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

    public ValueTask<T> Index(int slot) => _list.Index(slot);

    public ValueTask<T[]> Range(int start, int stop) => _list.Range(start, stop);

    public ValueTask<long> Push(params T[] arr) => _list.Push(arr);

    public ValueTask<long> Rem(int count, T obj) => _list.Rem(count, obj);
}