namespace QuantumCore.Caching.InMemory;

public class InMemoryRedisListWrapper<T> : IRedisListWrapper<T>
{
    private readonly List<T> _list = new();

    public ValueTask<T> Index(int slot)
    {
        return ValueTask.FromResult(_list[slot]);
    }

    public ValueTask<T[]> Range(int start, int stop)
    {
        var range = stop > 0
            ? start..stop
            : new Range(start, ^0);

        return ValueTask.FromResult(_list[range].ToArray());
    }

    public ValueTask<long> Push(params T[] arr)
    {
        _list.AddRange(arr);
        return ValueTask.FromResult<long>(arr.Length);
    }

    public ValueTask<long> Rem(int count, T obj)
    {
        _list.Remove(obj);
        return ValueTask.FromResult<long>(count);
    }

    public ValueTask<long> Len()
    {
        return ValueTask.FromResult<long>(_list.Count);
    }
}
