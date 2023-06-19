using System.Buffers;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace QuantumCore.Networking;

public class PacketReader : IPacketReader
{
    private readonly ILogger<PacketReader> _logger;
    private readonly INewPacketManager _newPacketManager;
    private readonly int _bufferSize;

    public PacketReader(ILogger<PacketReader> logger, INewPacketManager newPacketManager, IConfiguration configuration)
    {
        _logger = logger;
        _newPacketManager = newPacketManager;
        _bufferSize = configuration.GetValue<int?>("BufferSize") ?? 4096;
        _logger.LogDebug("Using buffer size {BufferSize}", _bufferSize);
    }
    
    public async IAsyncEnumerable<object> EnumerateAsync(Stream stream, [EnumeratorCancellation] CancellationToken token = default)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(_bufferSize);
        while (true)
        {
            // read header
            try
            {
                await stream.ReadExactlyAsync(buffer.AsMemory(0, 1), token);
            }
            catch (IOException)
            {
                _logger.LogInformation("Connection was most likely closed while reading a packet header");
                break;
            }
            var header = buffer[0];

            // read sub header
            byte? subHeader = null;
            if (_newPacketManager.IsSubPacketDefinition(header))
            {
                await stream.ReadExactlyAsync(buffer.AsMemory(1, 1), token);
                subHeader = buffer[1];
            }

            if (!_newPacketManager.TryGetPacketInfo(header, subHeader, out var packetInfo))
            {
                throw new NotImplementedException($"Received unknown header 0x{header:X2}");
            }

            // read full packet from stream
            IPacketSerializable packet;
            try
            {
                packet = (IPacketSerializable)await packetInfo.DeserializeFromStreamAsync(stream);
            }
            catch (ArgumentOutOfRangeException e)
            {
                throw new InvalidOperationException(
                    $"Buffer size {NetworkingConstants.BufferSize} is to small for package 0x{header:X2}. " +
                    $"You can increase the global buffer size by adjusting {nameof(NetworkingConstants)}.{nameof(NetworkingConstants.BufferSize)}",
                    e);
            }
            catch (IOException)
            {
                _logger.LogInformation("Connection was most likely closed while reading a packet");
                break;
            }

            yield return packet;

            if (packetInfo.HasSequence)
            {
                // read sequence to finalize the package read process
                await stream.ReadExactlyAsync(buffer.AsMemory(0, 1), token);
            }
        }
        ArrayPool<byte>.Shared.Return(buffer);
    }
}