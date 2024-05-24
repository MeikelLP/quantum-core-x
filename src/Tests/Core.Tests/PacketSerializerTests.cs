using QuantumCore.Networking;

namespace Core.Tests;

[ServerToClientPacket(0x01)]
public readonly ref partial struct MyPacket
{
    public readonly uint Size;
    public readonly ComplexSubType[] MyArray;
    public readonly int AnotherProperty;

    public MyPacket(ComplexSubType[] myArray, int anotherProperty)
    {
        Size = (uint) myArray.Length;
        MyArray = myArray;
        AnotherProperty = anotherProperty;
    }
}

public readonly struct ComplexSubType
{
    public readonly byte SubHeader;
    public readonly ushort Value;

    public ComplexSubType(byte subHeader, ushort value)
    {
        SubHeader = subHeader;
        Value = value;
    }
}

// public class PacketSerializerTests
// {
//     private readonly IPacketSerializer _serializer;
//
//     public PacketSerializerTests()
//     {
//         var services = new ServiceCollection()
//             .AddSingleton<IConfiguration>(_ => new ConfigurationBuilder()
//                 .AddInMemoryCollection(new Dictionary<string, string?>
//                 {
//                     { "Mode", "game" }
//                 })
//                 .Build())
//             .AddSingleton<IPacketManager>(provider => new PacketManager(provider.GetRequiredService<ILogger<PacketManager>>(), new []
//             {
//                 typeof(MyPacket)
//             }))
//             .AddSingleton<IPacketSerializer, DefaultPacketSerializer>()
//             .AddLogging()
//             .BuildServiceProvider();
//
//         _serializer = services.GetRequiredService<IPacketSerializer>();
//         services.GetRequiredService<IPacketManager>();
//     }
//
//     [Fact]
//     public void Serialize_ComplexTypeArray()
//     {
//         var data = new MyPacket(new[]
//             {
//                 new ComplexSubType(0x18, 0x0675),
//                 new ComplexSubType(0x43, 0x306E)
//             },
//             0x000004D2);
//
//         var bytes = _serializer.Serialize(data);
//
//         bytes.Should().BeEquivalentTo(new byte[]
//         {
//             0x01, // Header
//             0x0F, 0x00, 0x00, 0x00, // packet Size
//             0x18, 0x75, 0x06, // Array[0]
//             0x43, 0x06E, 0x30, // Array[0]
//             0xD2, 0x04, 0x00, 0x00, // AnotherProperty
//         });
//     }
// }