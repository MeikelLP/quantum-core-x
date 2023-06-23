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
    void Serialize(byte[] bytes, in int offset = 0);
    
    /// <summary>
    /// Deserializes from the given array and returns a new instance
    /// Assumes that the array starts after the header
    /// <remarks>This method should be avoided. There is a non generic variant as well which performances better memory-wise</remarks>
    /// </summary>
    /// <param name="bytes">Existing byte array to read from</param>
    /// <param name="offset">Start offset</param>
    static abstract T Deserialize<T>(ReadOnlySpan<byte> bytes, in int offset = 0)
        where T : IPacketSerializable;

    /// <summary>
    /// Deserializes from the given stream and returns a new instance
    /// Assumes that the stream has already read the header
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    static abstract ValueTask<object> DeserializeFromStreamAsync(Stream stream);

    /// <summary>
    /// Gets the header of the packet
    /// </summary>
    static abstract byte Header { get; }

    /// <summary>
    /// Gets the sub header of the packet if any
    /// </summary>
    static abstract byte? SubHeader { get; }

    /// <summary>
    /// Does not contain a dynamic field?
    /// May be string or array
    /// This is required to read bytes continuously while deserializing
    /// </summary>
    static abstract bool HasStaticSize { get; }
    
    /// <summary>
    /// Does this package have a sequence?
    /// A sequence is a terminating byte at the end of the package - usually \0
    /// </summary>
    static abstract bool HasSequence { get; }
}

public abstract class PacketBase : IPacketSerializable
{
    public ushort GetSize()
    {
        throw new NotImplementedException();
    }

    public void Serialize(byte[] bytes, in int offset = 0)
    {
        throw new NotImplementedException();
    }

    public static T Deserialize<T>(ReadOnlySpan<byte> bytes, in int offset = 0) where T : IPacketSerializable
    {
        throw new NotImplementedException();
    }

    public static ValueTask<object> DeserializeFromStreamAsync(Stream stream)
    {
        throw new NotImplementedException();
    }

    public static byte Header { get; }
    public static byte? SubHeader { get; }
    public static bool HasStaticSize { get; }
    public static bool HasSequence { get; }
}