namespace QuantumCore.Networking;

/// <summary>
/// Service to read packets from a (network) stream
/// </summary>
public interface IPacketReader
{
    /// <summary>
    /// Reads packets from a (network) stream.
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    /// <param name="token"></param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> which yields every time a new package is successfully read</returns>
    IAsyncEnumerable<object> EnumerateAsync(Stream stream, CancellationToken token = default);
}