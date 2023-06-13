namespace QuantumCore.Networking;

public interface IPacketSerializable
{
    /// <summary>
    /// Gets the full size of the current object.
    /// For non dynamic types this is a constant.
    /// For dynamic types this depends on the dynamic content.
    /// </summary>
    /// <returns></returns>
    ushort GetSize();
    
    /// <summary>
    /// Serializes the current object into the given byte array
    /// </summary>
    /// <param name="bytes">Existing byte array to write into</param>
    /// <param name="offset">Start offset</param>
    void Serialize(byte[] bytes, int offset = 0);
    
    /// <summary>
    /// Deserializes from the given array and returns a new instance
    /// Assumes that the array starts after the header
    /// <remarks>This method should be avoided. There is a non generic variant as well which performances better memory-wise</remarks>
    /// </summary>
    /// <param name="bytes">Existing byte array to read from</param>
    /// <param name="offset">Start offset</param>
    static abstract T Deserialize<T>(byte[] bytes, int offset = 0)
        where T : IPacketSerializable;
}