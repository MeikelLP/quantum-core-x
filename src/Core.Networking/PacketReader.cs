using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace QuantumCore.Networking;

public class PacketReader : IPacketReader
{
    private readonly ILogger<PacketReader> _logger;
    private readonly IPacketManager _packetManager;
    private readonly IHostEnvironment _env;
    private readonly int _bufferSize;

    public PacketReader(ILogger<PacketReader> logger, IPacketManager packetManager, IConfiguration configuration,
        IHostEnvironment env)
    {
        _logger = logger;
        _packetManager = packetManager;
        _env = env;
        _bufferSize = configuration.GetValue<int?>("BufferSize") ?? 4096;
        _logger.LogDebug("Using buffer size {BufferSize}", _bufferSize);
    }

    public async IAsyncEnumerable<object> EnumerateAsync(Stream stream,
        [EnumeratorCancellation] CancellationToken token = default)
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
                var headerString = Convert.ToString(header, 16);
                var subHeaderString = subHeader is not null ? $"|0x{Convert.ToString(subHeader.Value, 16)}" : "";
                if (_env.IsDevelopment())
                {
                    var bytes = await GetAsMuchDataAsPossibleAsync(stream);
                    _logger.LogDebug("Received unknown header: {Header}{SubHeader}. Payload: {Payload}", header,
                        subHeaderString, bytes);
                }

                throw new NotImplementedException($"Received unknown header 0x{headerString}{subHeaderString}");
            }

            // read full packet from stream
            IPacketSerializable packet;
            try
            {
                packet = (IPacketSerializable) await packetInfo.DeserializeFromStreamAsync(stream);
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    packet.Serialize(buffer);
                    var bytes = string.Join("", buffer[..packet.GetSize()].Select(x => x.ToString("X2")));
                    _logger.LogDebug(" IN: {Type} => {Packet} (0x{Bytes})", packet.GetType(),
                        JsonSerializer.Serialize<object>(packet), bytes);
                }
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

    /// <summary>
    /// Tries to read as many bytes as possible in 1s
    /// </summary>
    private async Task<byte[]> GetAsMuchDataAsPossibleAsync(Stream stream)
    {
        var bytes = new byte[1024];
        var waiter = Task.Delay(1000);
        var totalRead = 0;
        while (waiter.Status != TaskStatus.RanToCompletion)
        {
            // read one at a time or timeout
            var readTask = stream.ReadExactlyAsync(bytes, totalRead, 1).AsTask();
            await Task.WhenAny(waiter, readTask);
            totalRead++;
        }

        Array.Resize(ref bytes, totalRead);
        return bytes;
    }
}
