using QuantumCore.Networking;

namespace QuantumCore.Core.Networking;

public interface IPacketSerializer
{
    /// <summary>
    /// Serializes the given object to a byte array
    /// </summary>
    /// <param name="obj">The object to serialize. Must implement <see cref="IPacketSerializable"/></param>
    /// <typeparam name="T">You do not need to add this explicitly. It's just for a non-boxing struct parameter of any type</typeparam>
    /// <returns>A new byte array with the serialized data</returns>
    byte[] Serialize<T>(T obj) where T : IPacketSerializable;

    /// <summary>
    /// A memory efficient version of <see cref="Serialize{T}(T)"/>
    /// </summary>
    /// <param name="arr">An already existing array to write into</param>
    /// <param name="obj">The object to serialize. Must implement <see cref="IPacketSerializable"/></param>
    /// <param name="offset">An offset in the given array to write to. This is helpful if u want to write many at once</param>
    /// <typeparam name="T">You do not need to add this explicitly. It's just for a non-boxing struct parameter of any type</typeparam>
    void Serialize<T>(byte[] arr, T obj, int offset = 0) where T : IPacketSerializable;

    /// <summary>
    /// Only used in tests. Use <see cref="IPacketReader"/> in real life scenarios
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="offset"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T Deserialize<T>(byte[] bytes, int offset = 0) where T : IPacketSerializable;
}