using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using QuantumCore.Core.Networking;
using Xunit;

namespace Core.Tests;

public class PacketSerializerTests
{
    private readonly IPacketSerializer _serializer;

    [Packet(0x01, EDirection.Outgoing)]
    class MyPacket
    {
        [Field(0)]
        [Size]
        public uint Size { get; set; }
        
        [Field(1)]
        [Dynamic]
        public ComplexSubType[] MyArray { get; set; } = Array.Empty<ComplexSubType>();

        [Field(2)]
        public int AnotherProperty { get; set; }
    }

    class ComplexSubType
    {
        [Field(0)]
        public byte SubHeader { get; set; }
        
        [Field(1)]
        public ushort Value { get; set; }
    }

    public PacketSerializerTests()
    {
        var services = new ServiceCollection()
            .AddSingleton<IPacketManager, DefaultPacketManager>()
            .AddSingleton<IPacketSerializer, DefaultPacketSerializer>()
            .AddLogging()
            .BuildServiceProvider();

        _serializer = services.GetRequiredService<IPacketSerializer>();
        services.GetRequiredService<IPacketManager>().Register<MyPacket>();
    }

    [Fact]
    public void Serialize_ComplexTypeArray()
    {
        var data = new MyPacket
        {
            MyArray = new[]
            {
                new ComplexSubType{ SubHeader = 0x18, Value = 0x0675},
                new ComplexSubType{ SubHeader = 0x43, Value = 0x306E}
            },
            AnotherProperty = 0x4D2
        };

        var bytes = _serializer.Serialize(data);

        bytes.Should().BeEquivalentTo(new byte[]
        {
            0x01, // Header
            0x06, // Array Size
            0x18, 0x06, 0x75, // Array[0]
            0x43, 0x03, 0x06E, // Array[0]
            0x04, 0xD2 // AnotherProperty
        });
    }
}