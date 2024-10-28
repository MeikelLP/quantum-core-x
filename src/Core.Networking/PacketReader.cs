using System.Buffers;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace QuantumCore.Networking;

public class PacketReader : IPacketReader
{
    private readonly ILogger<PacketReader> _logger;
    private readonly IPacketManager _packetManager;
    private readonly int _bufferSize;

    public PacketReader([ServiceKey] string serviceKey, ILogger<PacketReader> logger, IConfiguration configuration,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _packetManager = serviceProvider.GetRequiredKeyedService<IPacketManager>(serviceKey);
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
            catch (ObjectDisposedException)
            {
                _logger.LogDebug("Connection was disposed while waiting or reading new packages. This may be fine.");
                break;
            }
            catch (IOException)
            {
                _logger.LogDebug("Connection was most likely closed while reading a packet. This may be fine");
                break;
            }
            var header = buffer[0];

            // read sub header
            byte? subHeader = null;

            try
            {
                if (_packetManager.IsSubPacketDefinition(header))
                {
                    await stream.ReadExactlyAsync(buffer.AsMemory(1, 1), token);
                    subHeader = buffer[1];
                }
            }
            catch (ObjectDisposedException)
            {
                _logger.LogDebug("Connection was disposed while waiting or reading new packages. This may be fine.");
                break;
            }
            catch (IOException)
            {
                _logger.LogDebug("Connection was most likely closed while reading a packet. This may be fine");
                break;
            }

            if (!_packetManager.TryGetPacketInfo(header, subHeader, out var packetInfo))
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
            catch (ObjectDisposedException)
            {
                _logger.LogDebug("Connection was disposed while waiting or reading new packages. This may be fine.");
                break;
            }
            catch (IOException)
            {
                _logger.LogDebug("Connection was most likely closed while reading a packet. This may be fine");
                break;
            }

            yield return packet;

            try
            {

                if (packetInfo.HasSequence)
                {
                    // read sequence to finalize the package read process
                    await stream.ReadExactlyAsync(buffer.AsMemory(0, 1), token);
                }
            }
            catch (ObjectDisposedException)
            {
                _logger.LogDebug("Connection was disposed while waiting or reading new packages. This may be fine.");
                break;
            }
            catch (IOException)
            {
                _logger.LogDebug("Connection was most likely closed while reading a packet. This may be fine");
                break;
            }
        }
        ArrayPool<byte>.Shared.Return(buffer);
    }
}
