using System;
using BeetleX.Redis;

namespace QuantumCore.Core.Cache;

internal class RedisSubscriber : IRedisSubscriber
{
    private readonly Subscriber _subscribe;

    public RedisSubscriber(Subscriber subscribe)
    {
        _subscribe = subscribe;
    }

    public void Register<T>(string channel, Action<T> action) => _subscribe.Register(channel, action);

    public void Listen() => _subscribe.Listen();
}