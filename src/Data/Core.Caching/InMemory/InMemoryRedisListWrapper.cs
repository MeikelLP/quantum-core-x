using QuantumCore.API;

namespace QuantumCore.Caching.InMemory;

public class InMemoryRedisListWrapper<T> : IRedisListWrapper<T>
{
    private readonly List<T> _list = [];

    public ValueTask<T> IndexAsync(int slot)
    {
        return ValueTask.FromResult(_list[slot]);
    }

    public ValueTask<T[]> RangeAsync(int start, int stop)
    {
        var range = stop > 0
            ? start..stop
            : new Range(start, ^0);

        return ValueTask.FromResult(_list[range].ToArray());
    }

    public ValueTask<long> PushAsync(params T[] arr)
    {
        ArgumentNullException.ThrowIfNull(arr);
        _list.AddRange(arr);
        return ValueTask.FromResult<long>(arr.Length);
    }

    public ValueTask<long> RemAsync(int count, T obj)
    {
        _list.Remove(obj);
        return ValueTask.FromResult<long>(count);
    }

    public ValueTask<long> LenAsync()
    {
        return ValueTask.FromResult<long>(_list.Count);
    }
}