using System;

namespace QuantumCore.Core.Networking;

public interface IPacketSerializer
{
    byte[] Serialize<T>(T obj);
    T Deserialize<T>(ReadOnlySpan<byte> bytes);
    object Deserialize(Type type, ReadOnlySpan<byte> bytes);
}