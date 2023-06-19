using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuantumCore.Core.Networking;
using QuantumCore.Networking;
using Xunit;

namespace Core.Tests;

[Packet(0x01, EDirection.Outgoing)]
[PacketGenerator]
public partial class MyPacket
{
    [Field(0)] [Size] public uint Size => (uint)MyArray.Length;
        
    [Field(1)]
    [Dynamic]
    public ComplexSubType[] MyArray { get; set; } = Array.Empty<ComplexSubType>();

    [Field(2)]
    public int AnotherProperty { get; set; }
}

public class ComplexSubType
{
    [Field(0)]
    public byte SubHeader { get; set; }
        
    [Field(1)]
    public ushort Value { get; set; }
}

public class PacketSerializerTests
{
    private readonly IPacketSerializer _serializer;

    public PacketSerializerTests()
    {
        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(_ => new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Mode", "game" }
                })
                .Build())
            .AddSingleton<IPacketManager>(provider => new PacketManager(provider.GetRequiredService<IConfiguration>(), new []
            {
                typeof(MyPacket)
            }))
            .AddSingleton<IPacketSerializer, DefaultPacketSerializer>()
            .AddLogging()
            .BuildServiceProvider();

        _serializer = services.GetRequiredService<IPacketSerializer>();
        services.GetRequiredService<IPacketManager>();
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
            AnotherProperty = 0x000004D2
        };

        var bytes = _serializer.Serialize(data);

        bytes.Should().BeEquivalentTo(new byte[]
        {
            0x01, // Header
            0x02, 0x00, 0x00, 0x00, // Array Size
            0x18, 0x06, 0x75, // Array[0]
            0x43, 0x03, 0x06E, // Array[0]
            0x00, 0x00, 0x04, 0xD2 // AnotherProperty
        });
    }
}