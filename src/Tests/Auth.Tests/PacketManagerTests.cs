using System.Net.Sockets;
using Auth.Tests.Extensions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using QuantumCore;
using QuantumCore.API;
using QuantumCore.Auth;
using QuantumCore.Auth.PacketHandlers;
using QuantumCore.Core.Packets;
using QuantumCore.Networking;
using Xunit;
using Xunit.Abstractions;

namespace Auth.Tests;

public class PacketManagerTests
{
    private readonly ITestOutputHelper _outputHelper;

    public PacketManagerTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    private PacketReader2<> GetPacketManager(params PacketInfo[] packetInfos)
    {
        var services = new ServiceCollection()
            .AddQuantumCoreTestLogger(_outputHelper)
            .AddSingleton(Substitute.For<IServerBase>())
            .AddSingleton(Substitute.For<IPluginExecutor>())
            .AddSingleton(Substitute.For<IPacketReader>())
            .AddSingleton(_ => new TcpClient())
            .AddSingleton<AuthConnection>()
            .BuildServiceProvider();
        return new PacketReader2<>(
            typeof(AuthPacketContext<>),
            services,
            packetInfos,
            services.GetRequiredService<ILogger<PacketReader2<>>>()
        );
    }

    [Fact]
    public void TryGetPacketInfo_ByInstance()
    {
        var packetInfo = new PacketInfo
        {
            HasSequence = false,
            StaticSize = 13,
            HandlerType = typeof(GCHandshakeHandler),
            PacketType = typeof(GCHandshake),
            PacketContextType = typeof(AuthPacketContext<GCHandshake>),
            Header = 0xFF
        };
        var result = GetPacketManager(packetInfo).TryGetPacketInfo(0xff, out var info);
        result.Should().BeTrue();
        info.Should().BeEquivalentTo(packetInfo);
    }

    [Fact]
    public void TryGetPacketInfo_NotFound()
    {
        var result = GetPacketManager(new PacketInfo
        {
            HasSequence = false,
            StaticSize = 13,
            HandlerType = typeof(GCHandshakeHandler),
            PacketType = typeof(GCHandshake),
            PacketContextType = typeof(AuthPacketContext<GCHandshake>),
            Header = 0xFF
        }).TryGetPacketInfo(0x44, out var info);
        result.Should().BeFalse();
        info.Should().BeNull();
    }

    [Fact]
    public async Task HandlePacket()
    {
        var packetManager = GetPacketManager(new PacketInfo
        {
            HasSequence = false,
            StaticSize = 13,
            HandlerType = typeof(CustomHandler),
            PacketType = typeof(GCHandshake),
            PacketContextType = typeof(AuthPacketContext<GCHandshake>),
            Header = 0xFF
        });

        packetManager.TryGetPacket(0xFF, [
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00
        ], out var packet).Should().BeTrue();

        await packetManager.HandlePacketAsync(0xFF, packet!);

        CustomHandler.IsHandled.Should().BeTrue();
    }
}

public class CustomHandler : IAuthPacketHandler<GCHandshake>
{
    public static bool IsHandled { get; set; }

    public ValueTask ExecuteAsync(AuthPacketContext<GCHandshake> context, CancellationToken token = default)
    {
        IsHandled = true;
        return ValueTask.CompletedTask;
    }
}