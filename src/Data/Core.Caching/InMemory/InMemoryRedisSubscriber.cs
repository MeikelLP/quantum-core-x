namespace QuantumCore.Caching.InMemory;

public class InMemoryRedisSubscriber : IRedisSubscriber
{
    public bool IsEnabled { get; private set; }
    public Dictionary<string, object> Callbacks { get; } = new();

    public void Register<T>(string channel, Action<T> action)
    {
        if (!Callbacks.TryGetValue(channel, out var list))
        {
            list = new List<Action<T>>();
            Callbacks.Add(channel, list);
        }

        ((List<Action<T>>) list).Add(action);
    }

    public void Listen()
    {
        IsEnabled = true;
    }
}