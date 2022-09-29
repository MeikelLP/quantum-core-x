using System;

namespace QuantumCore.Core.Cache;

public interface IRedisSubscriber
{
    void Register<T>(string channel, Action<T> action);
    void Listen();
}