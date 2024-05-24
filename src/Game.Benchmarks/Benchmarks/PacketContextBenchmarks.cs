using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore;
using QuantumCore.API;
using QuantumCore.Networking;

namespace Game.Benchmarks.Benchmarks;

public class PacketContextBenchmarks
{
    private readonly PacketContextFactory _packetContextFactory = new PacketContextFactory();
    private readonly IConnection _connection = new CustomAuthConnection();
    private readonly IPacket _packet = new GCHandshake2();
    private readonly ServiceProvider _provider;
    private readonly AsyncServiceScope _scope;

    // [Params(1, 10, 100, 1000)]
    public int Iterations { get; set; } = 1;

    [GlobalCleanup]
    public void Dispose()
    {
        _scope.Dispose();
    }

    public PacketContextBenchmarks()
    {
        _provider = new ServiceCollection()
            .AddLogging(cfg => cfg.ClearProviders())
            .AddScoped<IAuthConnection, CustomAuthConnection>()
            .AddScoped<CustomAuthConnection>()
            .AddScoped<CustomPacketHandler<GCHandshake2>>()
            .BuildServiceProvider();
        _scope = _provider.CreateAsyncScope();
    }


    [Benchmark]
    public void Context()
    {
        for (var i = 0; i < Iterations; i++)
        {
            var context = _packetContextFactory.GetContext(0xFF, _connection, _packet);
            _packetContextFactory.Return(0xFF, context);
        }
    }
}