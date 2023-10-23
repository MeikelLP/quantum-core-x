using System;
using System.Linq;
using System.Text;
using AutoBogus;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.Core.Packets;
using QuantumCore.Extensions;
using QuantumCore.Game.Packets;
using QuantumCore.Game.Packets.Quest;
using QuantumCore.Game.Packets.QuickBar;
using QuantumCore.Game.Packets.Shop;
using QuantumCore.Networking;
using Serilog;
using Weikio.PluginFramework.Catalogs;
using Xunit;
using Xunit.Abstractions;
using Version = QuantumCore.Game.Packets.Version;

namespace Core.Tests;

public class IncomingPacketTests
{
    private readonly IPacketSerializer _serializer;

    public IncomingPacketTests(ITestOutputHelper testOutputHelper)
    {
        var services = new ServiceCollection()
            .AddCoreServices(new EmptyPluginCatalog(), new ConfigurationBuilder().Build())
            .AddSingleton<IPacketSerializer, DefaultPacketSerializer>()
            .AddLogging(x =>
            {
                x.ClearProviders();
                x.AddSerilog(new LoggerConfiguration()
                    .WriteTo.TestOutput(testOutputHelper)
                    .CreateLogger());
            })
            .BuildServiceProvider();

        _serializer = services.GetRequiredService<IPacketSerializer>();
    }

    [Fact]
    public void WrongLengthThrowsArgumentException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _serializer.Deserialize<GCHandshake>(Array.Empty<byte>()));
    }

    [Fact]
    public void Attack()
    {
        var expected = new AutoFaker<Attack>()
            .RuleFor(x => x.Unknown, faker => new[]
            {
                faker.Random.Byte(),
                faker.Random.Byte()
            })
            .Generate();
        var bytes = Array.Empty<byte>()
            .Append(expected.AttackType)
            .Concat(BitConverter.GetBytes(expected.Vid))
            .Concat(expected.Unknown)
            .ToArray();

        var result = _serializer.Deserialize<Attack>(bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ChatIncoming()
    {
        var expected = new AutoFaker<ChatIncoming>().Generate();
        var bytes = Array.Empty<byte>()
            .Concat(BitConverter.GetBytes((ushort)(expected.Size + 1 + 3))) // size includes all package size + 0 terminating byte at end of string
            .Append((byte)expected.MessageType)
            .Concat(Encoding.ASCII.GetBytes(expected.Message))
            .Append((byte)0) // null byte for end of message
            .ToArray();

        var result = _serializer.Deserialize<ChatIncoming>(bytes);

        result.Should().BeEquivalentTo(expected);

        QuantumCore.Game.Packets.ChatIncoming.HasSequence.Should().BeTrue();
    }

    [Fact]
    public void CreateCharacter()
    {
        var expected = new AutoFaker<CreateCharacter>()
            .RuleFor(x => x.Name, faker => faker.Lorem.Letter(25))
            .RuleFor(x => x.Unknown, faker => new[]
            {
                faker.Random.Byte(),
                faker.Random.Byte(),
                faker.Random.Byte(),
                faker.Random.Byte()
            })
            .Generate();
        var bytes = Array.Empty<byte>()
            .Append(expected.Slot)
            .Concat(Encoding.ASCII.GetBytes(expected.Name))
            .Concat(BitConverter.GetBytes(expected.Class))
            .Append(expected.Appearance)
            .Concat(expected.Unknown)
            .ToArray();
        var result = _serializer.Deserialize<CreateCharacter>(bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void DeleteCharacter()
    {
        var expected = new AutoFaker<DeleteCharacter>()
            .RuleFor(x => x.Code, faker => faker.Lorem.Letter(8))
            .Generate();
        var bytes = Array.Empty<byte>()
            .Append(expected.Slot)
            .Concat(Encoding.ASCII.GetBytes(expected.Code))
            .ToArray();
        var result = _serializer.Deserialize<DeleteCharacter>(bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void SelectCharacter()
    {
        var expected = new AutoFaker<SelectCharacter>().Generate();
        var bytes = Array.Empty<byte>()
            .Append(expected.Slot)
            .ToArray();
        var result = _serializer.Deserialize<SelectCharacter>(bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void CharacterMove()
    {
        var expected = new AutoFaker<CharacterMove>().Generate();
        var bytes = Array.Empty<byte>()
            .Append(expected.MovementType)
            .Append(expected.Argument)
            .Append(expected.Rotation)
            .Concat(BitConverter.GetBytes(expected.PositionX))
            .Concat(BitConverter.GetBytes(expected.PositionY))
            .Concat(BitConverter.GetBytes(expected.Time))
            .ToArray();
        var result = _serializer.Deserialize<CharacterMove>(bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void EnterGame()
    {
        // var expected = new AutoFaker<EnterGame>().Generate();
        var bytes = Array.Empty<byte>()
            .ToArray();
        var result =  _serializer.Deserialize<EnterGame>(bytes);

        var ex = Assert.Throws<InvalidOperationException>(() => result.Should().BeEquivalentTo(new EnterGame()));
        ex.Message.Should()
            .BeEquivalentTo(
                "No members were found for comparison. Please specify some members to include in the comparison or choose a more meaningful assertion.");
    }

    [Fact]
    public void ItemUse()
    {
        var expected = new AutoFaker<ItemUse>().Generate();
        var bytes = Array.Empty<byte>()
            .Append(expected.Window)
            .Concat(BitConverter.GetBytes(expected.Position))
            .ToArray();
        var result = _serializer.Deserialize<ItemUse>(bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ItemMove()
    {
        var expected = new AutoFaker<ItemMove>().Generate();
        var bytes = Array.Empty<byte>()
            .Append(expected.FromWindow)
            .Concat(BitConverter.GetBytes(expected.FromPosition))
            .Append(expected.ToWindow)
            .Concat(BitConverter.GetBytes(expected.ToPosition))
            .Append(expected.Count)
            .ToArray();
        var result = _serializer.Deserialize<ItemMove>(bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ItemPickup()
    {
        var expected = new AutoFaker<ItemPickup>().Generate();
        var bytes = Array.Empty<byte>()
            .Concat(BitConverter.GetBytes(expected.Vid))
            .ToArray();
        var result = _serializer.Deserialize<ItemPickup>(bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void QuickBarAdd()
    {
        var expected = new AutoFaker<QuickBarAdd>().Generate();
        var bytes = Array.Empty<byte>()
            .Append(expected.Position)
            .Append(expected.Slot.Type)
            .Append(expected.Slot.Position)
            .ToArray();
        var result = _serializer.Deserialize<QuickBarAdd>(bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void QuickBarRemove()
    {
        var expected = new AutoFaker<QuickBarRemove>().Generate();
        var bytes = Array.Empty<byte>()
            .Append(expected.Position)
            .ToArray();
        var result = _serializer.Deserialize<QuickBarRemove>(bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void QuickBarSwap()
    {
        var expected = new AutoFaker<QuickBarSwap>().Generate();
        var bytes = Array.Empty<byte>()
            .Append(expected.Position1)
            .Append(expected.Position2)
            .ToArray();
        var result = _serializer.Deserialize<QuickBarSwap>(bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ItemDrop()
    {
        var expected = new AutoFaker<ItemDrop>().Generate();
        var bytes = Array.Empty<byte>()
            .Append(expected.Window)
            .Concat(BitConverter.GetBytes(expected.Position))
            .Concat(BitConverter.GetBytes(expected.Gold))
            .Append(expected.Count)
            .ToArray();
        var result = _serializer.Deserialize<ItemDrop>(bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ClickNpc()
    {
        var expected = new AutoFaker<ClickNpc>().Generate();
        var bytes = Array.Empty<byte>()
            .Concat(BitConverter.GetBytes(expected.Vid))
            .ToArray();
        var result = _serializer.Deserialize<ClickNpc>(bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void QuestAnswer()
    {
        var expected = new AutoFaker<QuestAnswer>().Generate();
        var bytes = Array.Empty<byte>()
            .Append(expected.Answer)
            .ToArray();
        var result = _serializer.Deserialize<QuestAnswer>(bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ShopClose()
    {
        // var expected = new AutoFaker<ShopClose>().Generate();
        var bytes = Array.Empty<byte>()
            .Append((byte)0x00)
            .ToArray();
        var result = _serializer.Deserialize<ShopClose>(bytes);

        var ex = Assert.Throws<InvalidOperationException>(() => result.Should().BeEquivalentTo(new ShopClose()));
        ex.Message.Should()
            .BeEquivalentTo(
                "No members were found for comparison. Please specify some members to include in the comparison or choose a more meaningful assertion.");
    }

    [Fact]
    public void ShopBuy()
    {
        var expected = new AutoFaker<ShopBuy>().Generate();
        var bytes = Array.Empty<byte>()
            .Append(expected.Count)
            .Append(expected.Position)
            .ToArray();
        var result = _serializer.Deserialize<ShopBuy>(bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ShopSell()
    {
        var expected = new AutoFaker<ShopSell>().Generate();
        var bytes = Array.Empty<byte>()
            .Append(expected.Position)
            .Append(expected.Count)
            .ToArray();
        var result = _serializer.Deserialize<ShopSell>(bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void TargetChange()
    {
        var expected = new AutoFaker<TargetChange>().Generate();
        var bytes = Array.Empty<byte>()
            .Concat(BitConverter.GetBytes(expected.TargetVid))
            .ToArray();
        var result = _serializer.Deserialize<TargetChange>(bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ItemGive()
    {
        var expected = new AutoFaker<ItemGive>().Generate();
        var bytes = Array.Empty<byte>()
            .Concat(BitConverter.GetBytes(expected.TargetVid))
            .Append(expected.Window)
            .Concat(BitConverter.GetBytes(expected.Position))
            .Append(expected.Count)
            .ToArray();
        var result = _serializer.Deserialize<ItemGive>(bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Empire()
    {
        var expected = new AutoFaker<Empire>().Generate();
        var bytes = Array.Empty<byte>()
            .Append(expected.EmpireId)
            .ToArray();
        var result = _serializer.Deserialize<Empire>(bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void TokenLogin()
    {
        var expected = new AutoFaker<TokenLogin>()
            .RuleFor(x => x.Username, faker => faker.Lorem.Letter(31))
            .RuleFor(x => x.Xteakeys, faker => new[]
            {
                faker.Random.UInt(),
                faker.Random.UInt(),
                faker.Random.UInt(),
                faker.Random.UInt()
            })
            .Generate();
        var bytes = Array.Empty<byte>()
            .Concat(Encoding.ASCII.GetBytes(expected.Username))
            .Concat(BitConverter.GetBytes(expected.Key))
            .Concat(expected.Xteakeys.SelectMany(BitConverter.GetBytes))
            .ToArray();
        var result = _serializer.Deserialize<TokenLogin>(bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Version()
    {
        var expected = new AutoFaker<Version>()
            .RuleFor(x => x.ExecutableName, faker => faker.Lorem.Letter(33))
            .RuleFor(x => x.Timestamp, faker => faker.Lorem.Letter(33))
            .Generate();
        var bytes = Array.Empty<byte>()
            .Concat(Encoding.ASCII.GetBytes(expected.ExecutableName))
            .Concat(Encoding.ASCII.GetBytes(expected.Timestamp))
            .ToArray();
        var result = _serializer.Deserialize<Version>(bytes);

        result.Should().BeEquivalentTo(expected);
    }
}
