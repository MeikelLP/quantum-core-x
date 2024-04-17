namespace QuantumCore.Networking;

public interface IPacketSerializable
{
    /// <summary>
    /// Gets the full size of the current object.
    /// For non-dynamic types this is a constant.
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
    /// Deserializes from the given array and populates this instance
    /// Assumes that the array starts after the header
    /// <remarks>This method should be avoided. There is a non generic variant as well which performances better memory-wise</remarks>
    /// </summary>
    /// <param name="bytes">Existing byte array to read from</param>
    /// <param name="offset">Start offset</param>
    void Deserialize(ReadOnlySpan<byte> bytes, in int offset = 0);

    /// <summary>
    /// Deserializes from the given stream and populates this instance
    /// Assumes that the stream has already read the header
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    ValueTask DeserializeFromStreamAsync(Stream stream);

    /// <summary>
    /// Gets the header of the packet
    /// </summary>
    byte Header { get; }

    /// <summary>
    /// Gets the sub header of the packet if any
    /// </summary>
    byte? SubHeader { get; }

    /// <summary>
    /// Does not contain a dynamic field?
    /// May be string or array
    /// This is required to read bytes continuously while deserializing
    /// </summary>
    bool HasStaticSize { get; }

    /// <summary>
    /// Does this package have a sequence?
    /// A sequence is a terminating byte at the end of the package - usually \0
    /// </summary>
    bool HasSequence { get; }
}
