namespace QuantumCore.Caching;

public interface ICacheManager: IRedisStore
{
    public IRedisStore Shared {get;}
    public IRedisStore Server {get;}
}
