using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using QuantumCore;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.Types.Skills;
using QuantumCore.API.Packets;
using QuantumCore.API.Packets.Shop;
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
                    { "Mode", HostingOptions.MODE_GAME }, { "BufferSize", bufferSize.ToString() }
                })
                .Build())
            .AddLogging()
            .AddSingleton<IHostEnvironment>(_ => new HostingEnvironment())
            .AddKeyedSingleton<IPacketManager>(HostingOptions.MODE_GAME, (provider, _) =>
            {
                return new PacketManager(provider.GetRequiredService<ILogger<PacketManager>>(),
                    [typeof(Attack), typeof(CharacterDead), typeof(ChatIncoming), typeof(ShopBuy)]);
            })
            .AddKeyedSingleton<IPacketReader, PacketReader>(HostingOptions.MODE_GAME)
            .BuildServiceProvider();
        return services.GetRequiredKeyedService<IPacketReader>(HostingOptions.MODE_GAME);
    }

    [Fact]
    public async Task SimpleAsync()
    {
        var obj = new Attack { Unknown = [0, 0], Vid = 1_000_000, SkillMotion = (ESkill)53 };
        var size = obj.GetSize();
        var bytes = new byte[size];
        obj.Serialize(bytes);

        using var stream = new MemoryStream(bytes);
        var results = await GetReader(16).EnumerateAsync(stream, TestContext.Current.CancellationToken)
            .ToArrayAsync(cancellationToken: TestContext.Current.CancellationToken);

        results.Should().HaveCount(1);
        results[0].Should().BeEquivalentTo(obj);
    }

    [Fact]
    public async Task SubPacketAsync()
    {
        var obj = new ShopBuy { Position = 24, Count = 10 };
        var size = obj.GetSize();
        var bytes = new byte[size];
        obj.Serialize(bytes);

        using var stream = new MemoryStream(bytes);
        var results = await GetReader().EnumerateAsync(stream, TestContext.Current.CancellationToken)
            .ToArrayAsync(cancellationToken: TestContext.Current.CancellationToken);

        results.Should().HaveCount(1);
        results[0].Should().BeEquivalentTo(obj);
    }

    [Fact]
    public async Task MultipleWithSequenceAsync()
    {
        var obj = new Attack { Vid = 1_000_000, SkillMotion = (ESkill)5, Unknown = [0, 0] };
        var size = obj.GetSize();
        var bytes = new byte[size * 2];
        obj.Serialize(bytes);
        obj.Serialize(bytes, size);

        using var stream = new MemoryStream(bytes);
        var results = await GetReader().EnumerateAsync(stream, TestContext.Current.CancellationToken)
            .ToArrayAsync(cancellationToken: TestContext.Current.CancellationToken);

        results.Should().HaveCount(2);
        results.Should().AllBeEquivalentTo(obj);
    }

    [Fact]
    public async Task DynamicAsync()
    {
        var obj = new ChatIncoming { MessageType = ChatMessageType.NORMAL, Message = "Hello New World!" };
        var size = obj.GetSize();
        var bytes = new byte[size + 1]; // + 1 due to sequence
        obj.Serialize(bytes);

        using var stream = new MemoryStream(bytes);
        var results = await GetReader(4096).EnumerateAsync(stream, TestContext.Current.CancellationToken)
            .ToArrayAsync(cancellationToken: TestContext.Current.CancellationToken);

        results.Should().HaveCount(1);
        results[0].Should().BeEquivalentTo(obj);
    }

    [Fact]
    public async Task BufferToSmallAsync()
    {
        var obj = new ChatIncoming
        {
            MessageType = ChatMessageType.NORMAL,
            Message = new string(Enumerable.Range(0, 5000).Select(_ => 'i').ToArray())
        };
        var size = obj.GetSize();
        var bytes = new byte[size];
        obj.Serialize(bytes);

        using var stream = new MemoryStream(bytes);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            GetReader(4).EnumerateAsync(stream, TestContext.Current.CancellationToken)
                .ToArrayAsync(cancellationToken: TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task MultipleAsync()
    {
        var obj = new Attack { Unknown = [0, 0], Vid = 1_000_000, SkillMotion = (ESkill)53 };
        var size = obj.GetSize();
        var bytes = new byte[size + size];
        obj.Serialize(bytes);
        obj.Serialize(bytes, size);

        using var stream = new MemoryStream(bytes);
        var results = await GetReader(16).EnumerateAsync(stream, TestContext.Current.CancellationToken)
            .ToArrayAsync(cancellationToken: TestContext.Current.CancellationToken);

        results.Should().HaveCount(2);
        results[0].Should().BeEquivalentTo(obj);
        results[1].Should().BeEquivalentTo(obj);
    }

    [Fact]
    public async Task MoreThanBufferAsync()
    {
        var obj = new Attack { Unknown = [0, 0], Vid = 1_000_000, SkillMotion = (ESkill)53 };
        var size = obj.GetSize();
        var bytes = new byte[size * 3];
        obj.Serialize(bytes);
        obj.Serialize(bytes, size);
        obj.Serialize(bytes, size * 2);

        using var stream = new MemoryStream(bytes);
        var results = await GetReader(16).EnumerateAsync(stream, TestContext.Current.CancellationToken)
            .ToArrayAsync(cancellationToken: TestContext.Current.CancellationToken);

        results.Should().HaveCount(3);
        results[0].Should().BeEquivalentTo(obj);
        results[1].Should().BeEquivalentTo(obj);
        results[2].Should().BeEquivalentTo(obj);
    }

    [Fact]
    public async Task OddSizeAsync()
    {
        var obj = new CharacterDead { Vid = 1_000_000 };
        var size = obj.GetSize();
        var bytes = new byte[size * 10];
        for (var i = 0; i < 10; i++)
        {
            obj.Serialize(bytes, size * i);
        }

        using var stream = new MemoryStream(bytes);
        var results = await GetReader(16).EnumerateAsync(stream, TestContext.Current.CancellationToken)
            .ToArrayAsync(cancellationToken: TestContext.Current.CancellationToken);

        results.Should().HaveCount(10);
        results.Should().AllBeEquivalentTo(obj);
    }

    [Fact]
    public async Task DifferentPacketsAsync()
    {
        var charDeadObj = new CharacterDead { Vid = 1_000_000 };
        var attackObj = new Attack { Unknown = [0, 0], Vid = 1_000_000, SkillMotion = (ESkill)53 };
        var charDeadSize = charDeadObj.GetSize();
        var attackSize = attackObj.GetSize();
        var bytes = new byte[charDeadSize + attackSize];
        charDeadObj.Serialize(bytes);
        attackObj.Serialize(bytes, charDeadSize);

        using var stream = new MemoryStream(bytes);
        var results = await GetReader().EnumerateAsync(stream, TestContext.Current.CancellationToken)
            .ToArrayAsync(cancellationToken: TestContext.Current.CancellationToken);

        results.Should().HaveCount(2);
        results[0].Should().BeEquivalentTo(charDeadObj);
        results[1].Should().BeEquivalentTo(attackObj);
    }
}