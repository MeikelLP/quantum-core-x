namespace QuantumCore.Caching;

public interface IRedisListWrapper
{
    ValueTask<long> Len();
}

public interface IRedisListWrapper<T> : IRedisListWrapper
{
    ValueTask<T> Index(int slot);
    ValueTask<T[]> Range(int start, int stop);
    ValueTask<long> Push(params T[] arr);
    ValueTask<long> Rem(int count, T obj);
}
