namespace QuantumCore.API;

public interface ICacheManager : IRedisStore
{
    public IRedisStore Shared { get; }
    public IRedisStore Server { get; }
}
