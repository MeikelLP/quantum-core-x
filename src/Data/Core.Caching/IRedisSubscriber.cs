namespace QuantumCore.Caching;

public interface IRedisSubscriber
{
    void Register<T>(string channel, Action<T> action);
    void Listen();
}