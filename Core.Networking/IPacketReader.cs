namespace QuantumCore.Networking;

public interface IPacketReader
{
    IAsyncEnumerable<object> EnumerateAsync(Stream stream, CancellationToken token = default);
}