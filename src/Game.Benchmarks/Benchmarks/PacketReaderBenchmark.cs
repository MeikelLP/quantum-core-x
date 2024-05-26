using System.Buffers;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;
using QuantumCore.Networking;

namespace Game.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class PacketReaderBenchmark
{
    private readonly PacketReader2<CustomAuthConnection> _packetReader2;
    private readonly IPacketReader _packetReader;
    private readonly PacketContextFactory _packetContextFactory = new();
    private readonly AsyncServiceScope _scope;
    private readonly MemoryStream _stream;

    // [Params(1, 10, 100, 1000)]
    public int Iterations { get; set; } = 1;

    [GlobalCleanup]
    public void Dispose()
    {
        _scope.Dispose();
    }

    public PacketReaderBenchmark()
    {
        var provider = new ServiceCollection()
            .AddLogging(cfg => cfg.ClearProviders())
            .AddScoped<IAuthConnection, CustomAuthConnection>()
            .AddScoped<CustomAuthConnection>()
            .AddScoped<CustomPacketHandler<GCHandshake2>>()
            .AddScoped<IConfiguration>(_ => new ConfigurationBuilder().Build())
            .AddScoped<IPacketReader, PacketReader>()
            .AddSingleton<IPacketManager>(provider => new PacketManager(
                provider.GetRequiredService<ILogger<PacketManager>>(),
                [typeof(GCHandshake)],
                [typeof(GCHandshakeHandler)])
            )
            .BuildServiceProvider();
        _packetReader = provider.GetRequiredService<IPacketReader>();
        _packetReader2 =
            new PacketReader2<CustomAuthConnection>([
                    new PacketInfo2
                    {
                        HasSequence = false,
                        StaticSize = 13,
                        HandlerType = typeof(CustomPacketHandler<GCHandshake2>),
                        PacketType = typeof(GCHandshake2),
                        Header = 0xFF
                    }
                ], provider.GetRequiredService<ILogger<PacketReader2<CustomAuthConnection>>>(),
                _packetContextFactory);
        _scope = provider.CreateAsyncScope();
        _stream = new MemoryStream([
            0xFF,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00
        ]);
    }

    [Benchmark(Baseline = true)]
    public async ValueTask Handler()
    {
        for (var i = 0; i < Iterations; i++)
        {
            await foreach (var packet in _packetReader.EnumerateAsync(_stream))
            {
            }

            _stream.Position = 0;
        }
    }

    [Benchmark]
    public async ValueTask Handler2()
    {
        for (var i = 0; i < Iterations; i++)
        {
            _packetReader2.TryGetPacket(0xFF, [
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00
            ], out var packet);
            await _packetReader2.HandlePacketAsync(_scope.ServiceProvider, 0xFF, packet!);
        }
    }
}

#region Types

class CustomAuthConnection : IAuthConnection
{
    public Guid Id { get; }
    public EPhases Phase { get; set; }
    public Task ExecuteTask { get; }

    public void Close(bool expected = true)
    {
    }

    public void Send(byte[] packet)
    {
        throw new NotImplementedException();
    }

    public Task StartAsync(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public bool HandleHandshake(GCHandshakeData handshake)
    {
        return true;
    }
}

class CustomPacketHandler<TPacket> : IPacketHandler<CustomAuthConnection, TPacket>
    where TPacket : IPacket
{
    public ValueTask ExecuteAsync(PacketContext<CustomAuthConnection, TPacket> context,
        CancellationToken token = default)
    {
        return ValueTask.CompletedTask;
    }
}

[ClientToServerPacket(0xff)]
[ServerToClientPacket(0xff)]
public partial class GCHandshake2
{
    public uint Handshake { get; set; }
    public uint Time { get; set; }
    public uint Delta { get; set; }
}

public class GCHandshake : IPacketSerializable
{
    public uint Handshake { get; set; }
    public uint Time { get; set; }
    public uint Delta { get; set; }
    public ushort GetSize() => 12;

    public void Serialize(byte[] bytes, in int offset = 0)
    {
        bytes[offset + 0] = 0xFF;
        bytes[offset + 1] = (System.Byte) (this.Handshake >> 0);
        bytes[offset + 2] = (System.Byte) (this.Handshake >> 8);
        bytes[offset + 3] = (System.Byte) (this.Handshake >> 16);
        bytes[offset + 4] = (System.Byte) (this.Handshake >> 24);
        bytes[offset + 5] = (System.Byte) (this.Time >> 0);
        bytes[offset + 6] = (System.Byte) (this.Time >> 8);
        bytes[offset + 7] = (System.Byte) (this.Time >> 16);
        bytes[offset + 8] = (System.Byte) (this.Time >> 24);
        bytes[offset + 9] = (System.Byte) (this.Delta >> 0);
        bytes[offset + 10] = (System.Byte) (this.Delta >> 8);
        bytes[offset + 11] = (System.Byte) (this.Delta >> 16);
        bytes[offset + 12] = (System.Byte) (this.Delta >> 24);
    }

    public static T Deserialize<T>(ReadOnlySpan<byte> bytes, in int offset = 0) where T : IPacketSerializable
    {
        var __Handshake = System.BitConverter.ToUInt32(bytes[0..(0 + 4)]);
        var __Time = System.BitConverter.ToUInt32(bytes[4..(4 + 4)]);
        var __Delta = System.BitConverter.ToUInt32(bytes[8..(8 + 4)]);
        return (T) (object) new GCHandshake
        {
            Handshake = __Handshake,
            Delta = __Delta,
            Time = __Time
        };
    }

    public static async ValueTask<object> DeserializeFromStreamAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(1024);
        var __Handshake = await stream.ReadValueFromStreamAsync<uint>(buffer);
        var __Time = await stream.ReadValueFromStreamAsync<uint>(buffer);
        var __Delta = await stream.ReadValueFromStreamAsync<uint>(buffer);
        ArrayPool<byte>.Shared.Return(buffer);
        return new GCHandshake
        {
            Handshake = __Handshake,
            Delta = __Delta,
            Time = __Time
        };
    }

    public static byte Header => 0xFF;
    public static byte? SubHeader => null;
    public static bool HasStaticSize => true;
    public static bool HasSequence => false;
}

public class GCHandshakeHandler : ICustomPacketHandler<GCHandshake>
{
    public ValueTask ExecuteAsync(CustomPacketContext<GCHandshake> context, CancellationToken token = default)
    {
        context.Connection.HandleHandshake(new GCHandshakeData
        {
            Delta = context.Packet.Delta,
            Handshake = context.Packet.Handshake,
            Time = context.Packet.Time
        });

        return ValueTask.CompletedTask;
    }
}

public interface ICustomPacketHandler<T>
{
}

public interface CustomPacketContext<T>
{
    IConnection Connection { get; }
    T Packet { get; }
}

#endregion Types