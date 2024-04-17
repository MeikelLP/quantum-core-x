using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using QuantumCore.Core.Packets;
using QuantumCore.Game.PacketHandlers;
using QuantumCore.Networking;

namespace Game.Tests;

public class PacketManagerTests
{
    [Fact]
    public void TryGetPacketInfo_ByInstance()
    {
        var packetManager = new PacketManager(
            Substitute.For<ILogger<PacketManager>>(),
            [typeof(GCHandshake)],
            [typeof(GameGCHandshakeHandler)]
        );

        var result = packetManager.TryGetPacketInfo(new GCHandshake(), out var info);
        result.Should().BeTrue();
        info.Should().BeEquivalentTo(new PacketInfo(typeof(GCHandshake), typeof(GameGCHandshakeHandler), true, false));
    }

    [Fact]
    public void TryGetPacketInfo_ByHeader()
    {
        var packetManager = new PacketManager(
            Substitute.For<ILogger<PacketManager>>(),
            [typeof(GCHandshake)],
            [typeof(GameGCHandshakeHandler)]
        );

        var result = packetManager.TryGetPacketInfo(0xff, null, out var info);
        result.Should().BeTrue();
        info.Should().BeEquivalentTo(new PacketInfo(typeof(GCHandshake), typeof(GameGCHandshakeHandler), true, false));
    }

    [Fact]
    public void TryGetPacketInfo_NotFound()
    {
        var packetManager = new PacketManager(
            Substitute.For<ILogger<PacketManager>>(),
            [typeof(GCHandshake)],
            [typeof(GameGCHandshakeHandler)]
        );

        var result = packetManager.TryGetPacketInfo(0x44, null, out var info);
        result.Should().BeFalse();
        info.Should().BeEquivalentTo(new PacketInfo());
    }
}
