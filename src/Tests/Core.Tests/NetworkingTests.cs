using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using QuantumCore.Game;
using QuantumCore.Game.Packets;
using QuantumCore.Game.Packets.Shop;
using QuantumCore.Networking;
using Xunit;

namespace Core.Tests;

public class NetworkingTests
{
    private static IPacketReader GetReader(int bufferSize = 32)
    {
        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(_ => new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Mode", "game" },
                    { "BufferSize", bufferSize.ToString() }
                })
                .Build())
            .AddLogging()
            .AddSingleton<IHostEnvironment>(_ => new HostingEnvironment())
            .AddSingleton<IPacketManager>(provider =>
            {
                return new PacketManager(provider.GetRequiredService<ILogger<PacketManager>>(), new[]
                {
                    typeof(Attack),
                    typeof(CharacterDead),
                    typeof(ChatIncoming),
                    typeof(ShopBuy)
                });
            })
            .AddSingleton<IPacketReader, PacketReader>()
            .BuildServiceProvider();
        return services.GetRequiredService<IPacketReader>();
    }
    
    [Fact]
    public async Task Simple()
    {
        var obj = new Attack
        {
            Unknown = new byte [] {0, 0},
            Vid = 1_000_000,
            AttackType = 53
        };
        var size = obj.GetSize();
        var bytes = new byte[size];
        obj.Serialize(bytes);

        using var stream = new MemoryStream(bytes);
        var results = await GetReader(16).EnumerateAsync(stream).ToArrayAsync(); 

        results.Should().HaveCount(1);
        results[0].Should().BeEquivalentTo(obj);
    }
    
    [Fact]
    public async Task SubPacket()
    {
        var obj = new ShopBuy
        {
            Position = 24,
            Count = 10
        };
        var size = obj.GetSize();
        var bytes = new byte[size];
        obj.Serialize(bytes);

        using var stream = new MemoryStream(bytes);
        var results = await GetReader().EnumerateAsync(stream).ToArrayAsync(); 

        results.Should().HaveCount(1);
        results[0].Should().BeEquivalentTo(obj);
    }
    
    [Fact]
    public async Task MultipleWithSequence()
    {
        var obj = new Attack
        {
            Vid = 1_000_000,
            AttackType = 5,
            Unknown = new byte[]{0,0}
        };
        var size = obj.GetSize();
        var bytes = new byte[size * 2];
        obj.Serialize(bytes);
        obj.Serialize(bytes, size);

        using var stream = new MemoryStream(bytes);
        var results = await GetReader().EnumerateAsync(stream).ToArrayAsync(); 

        results.Should().HaveCount(2);
        results.Should().AllBeEquivalentTo(obj);
    }
    
    [Fact]
    public async Task Dynamic()
    {
        var obj = new ChatIncoming
        {
            MessageType = ChatMessageTypes.Normal,
            Message = "Hello New World!"
        };
        var size = obj.GetSize();
        var bytes = new byte[size + 1]; // + 1 due to sequence
        obj.Serialize(bytes);

        using var stream = new MemoryStream(bytes);
        var results = await GetReader(4096).EnumerateAsync(stream).ToArrayAsync(); 

        results.Should().HaveCount(1);
        results[0].Should().BeEquivalentTo(obj);
    }
    
    [Fact]
    public async Task BufferToSmall()
    {
        var obj = new ChatIncoming
        {
            MessageType = ChatMessageTypes.Normal,
            Message = new string(Enumerable.Range(0, 5000).Select(x => 'i').ToArray())
        };
        var size = obj.GetSize();
        var bytes = new byte[size];
        obj.Serialize(bytes);

        using var stream = new MemoryStream(bytes);
        
        await Assert.ThrowsAsync<InvalidOperationException>(() => GetReader(4).EnumerateAsync(stream).ToArrayAsync().AsTask());
    }
    
    [Fact]
    public async Task Multiple()
    {
        var obj = new Attack
        {
            Unknown = new byte [] {0, 0},
            Vid = 1_000_000,
            AttackType = 53
        };
        var size = obj.GetSize();
        var bytes = new byte[size + size];
        obj.Serialize(bytes);
        obj.Serialize(bytes, size);

        using var stream = new MemoryStream(bytes);
        var results = await GetReader(16).EnumerateAsync(stream).ToArrayAsync(); 

        results.Should().HaveCount(2);
        results[0].Should().BeEquivalentTo(obj);
        results[1].Should().BeEquivalentTo(obj);
    }
    
    [Fact]
    public async Task MoreThanBuffer()
    {
        var obj = new Attack
        {
            Unknown = new byte [] {0, 0},
            Vid = 1_000_000,
            AttackType = 53
        };
        var size = obj.GetSize();
        var bytes = new byte[size * 3];
        obj.Serialize(bytes);
        obj.Serialize(bytes, size);
        obj.Serialize(bytes, size * 2);

        using var stream = new MemoryStream(bytes);
        var results = await GetReader(16).EnumerateAsync(stream).ToArrayAsync(); 

        results.Should().HaveCount(3);
        results[0].Should().BeEquivalentTo(obj);
        results[1].Should().BeEquivalentTo(obj);
        results[2].Should().BeEquivalentTo(obj);
    }
    
    [Fact]
    public async Task OddSize()
    {
        var obj = new CharacterDead
        {
            Vid = 1_000_000
        };
        var size = obj.GetSize();
        var bytes = new byte[size * 10];
        for (var i = 0; i < 10; i++)
        {
            obj.Serialize(bytes, size * i);
        }

        using var stream = new MemoryStream(bytes);
        var results = await GetReader(16).EnumerateAsync(stream).ToArrayAsync(); 

        results.Should().HaveCount(10);
        results.Should().AllBeEquivalentTo(obj);
    }
    
    [Fact]
    public async Task DifferentPackets()
    {
        var charDeadObj = new CharacterDead
        {
            Vid = 1_000_000
        };
        var attackObj = new Attack
        {
            Unknown = new byte [] {0, 0},
            Vid = 1_000_000,
            AttackType = 53
        };
        var charDeadSize = charDeadObj.GetSize();
        var attackSize = attackObj.GetSize();
        var bytes = new byte[charDeadSize + attackSize];
        charDeadObj.Serialize(bytes);
        attackObj.Serialize(bytes, charDeadSize);

        using var stream = new MemoryStream(bytes);
        var results = await GetReader(32).EnumerateAsync(stream).ToArrayAsync(); 

        results.Should().HaveCount(2);
        results[0].Should().BeEquivalentTo(charDeadObj);
        results[1].Should().BeEquivalentTo(attackObj);
    }
}