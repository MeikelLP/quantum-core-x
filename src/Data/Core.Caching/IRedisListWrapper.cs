namespace QuantumCore.Caching;

public interface IRedisListWrapper<T>
{
    ValueTask<T> Index(int slot);
    ValueTask<T[]> Range(int start, int stop);
    ValueTask<long> Push(params T[] arr);
    ValueTask<long> Rem(int count, T obj);
}