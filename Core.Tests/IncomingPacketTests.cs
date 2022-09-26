using System;
using System.Linq;
using System.Text;
using AutoBogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.Core.Networking;
using QuantumCore.Extensions;
using QuantumCore.Game.Packets;
using QuantumCore.Game.Packets.Quest;
using QuantumCore.Game.Packets.QuickBar;
using QuantumCore.Game.Packets.Shop;
using Serilog;
using Weikio.PluginFramework.Catalogs;
using Xunit;
using Xunit.Abstractions;
using Version = QuantumCore.Game.Packets.Version;

namespace Core.Tests;

public class IncomingPacketTests
{
    private readonly IPacketManager _packetManager;

    public IncomingPacketTests(ITestOutputHelper testOutputHelper)
    {
        var services = new ServiceCollection()
            .AddCoreServices(new EmptyPluginCatalog())
            .AddLogging(x =>
            {
                x.ClearProviders();
                x.AddSerilog(new LoggerConfiguration()
                    .WriteTo.TestOutput(testOutputHelper)
                    .CreateLogger());
            })
            .BuildServiceProvider();
        _packetManager = services.GetRequiredService<IPacketManager>();
        _packetManager.RegisterNamespace("QuantumCore.Game.Packets", typeof(ItemMove).Assembly);
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

        var packetCache = _packetManager.GetIncomingPacket(0x02);
        var result = new Attack();
        packetCache.Deserialize(result, bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ChatIncoming()
    {
        var expected = new AutoFaker<ChatIncoming>().Generate();
        var bytes1 = Array.Empty<byte>()
            .Concat(BitConverter.GetBytes(expected.Size))
            .Append((byte)expected.MessageType)
            .ToArray();
        var bytes2 = Array.Empty<byte>()
            .Concat(Encoding.ASCII.GetBytes(expected.Message))
            .Append((byte)0) // null byte for end of message
            .ToArray();

        var packetCache = _packetManager.GetIncomingPacket(0x03);
        var result = new ChatIncoming();
        packetCache.Deserialize(result, bytes1);
        packetCache.DeserializeDynamic(result, bytes2);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void CreateCharacter()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x04);
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
        var result = new CreateCharacter();
        packetCache.Deserialize(result, bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void DeleteCharacter()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x05);
        var expected = new AutoFaker<DeleteCharacter>()
            .RuleFor(x => x.Code, faker => faker.Lorem.Letter(8))
            .Generate();
        var bytes = Array.Empty<byte>()
            .Append(expected.Slot)
            .Concat(Encoding.ASCII.GetBytes(expected.Code))
            .ToArray();
        var result = new DeleteCharacter();
        packetCache.Deserialize(result, bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void SelectCharacter()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x06);
        var expected = new AutoFaker<SelectCharacter>().Generate();
        var bytes = Array.Empty<byte>()
            .Append(expected.Slot)
            .ToArray();
        var result = new SelectCharacter();
        packetCache.Deserialize(result, bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void CharacterMove()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x07);
        var expected = new AutoFaker<CharacterMove>().Generate();
        var bytes = Array.Empty<byte>()
            .Append(expected.MovementType)
            .Append(expected.Argument)
            .Append(expected.Rotation)
            .Concat(BitConverter.GetBytes(expected.PositionX))
            .Concat(BitConverter.GetBytes(expected.PositionY))
            .Concat(BitConverter.GetBytes(expected.Time))
            .ToArray();
        var result = new CharacterMove();
        packetCache.Deserialize(result, bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void EnterGame()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x0a);
        // var expected = new AutoFaker<EnterGame>().Generate();
        var bytes = Array.Empty<byte>()
            .ToArray();
        var result = new EnterGame();
        packetCache.Deserialize(result, bytes);

        // not useful as long as there are no properties
        // result.Should().BeEquivalentTo(expected);
        
        Assert.Empty(typeof(EnterGame).GetProperties());
    }

    [Fact]
    public void ItemUse()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x0b);
        var expected = new AutoFaker<ItemUse>().Generate();
        var bytes = Array.Empty<byte>()
            .Append(expected.Window)
            .Concat(BitConverter.GetBytes(expected.Position))
            .ToArray();
        var result = new ItemUse();
        packetCache.Deserialize(result, bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ItemMove()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x0d);
        var expected = new AutoFaker<ItemMove>().Generate();
        var bytes = Array.Empty<byte>()
            .Append(expected.FromWindow)
            .Concat(BitConverter.GetBytes(expected.FromPosition))
            .Append(expected.ToWindow)
            .Concat(BitConverter.GetBytes(expected.ToPosition))
            .Append(expected.Count)
            .ToArray();
        var result = new ItemMove();
        packetCache.Deserialize(result, bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ItemPickup()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x0F);
        var expected = new AutoFaker<ItemPickup>().Generate();
        var bytes = Array.Empty<byte>()
            .Concat(BitConverter.GetBytes(expected.Vid))
            .ToArray();
        var result = new ItemPickup();
        packetCache.Deserialize(result, bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void QuickBarAdd()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x10);
        var expected = new AutoFaker<QuickBarAdd>().Generate();
        var bytes = Array.Empty<byte>()
            .Append(expected.Position)
            .Append(expected.Slot.Type)
            .Append(expected.Slot.Position)
            .ToArray();
        var result = new QuickBarAdd();
        packetCache.Deserialize(result, bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void QuickBarRemove()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x11);
        var expected = new AutoFaker<QuickBarRemove>().Generate();
        var bytes = Array.Empty<byte>()
            .Append(expected.Position)
            .ToArray();
        var result = new QuickBarRemove();
        packetCache.Deserialize(result, bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void QuickBarSwap()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x12);
        var expected = new AutoFaker<QuickBarSwap>().Generate();
        var bytes = Array.Empty<byte>()
            .Append(expected.Position1)
            .Append(expected.Position2)
            .ToArray();
        var result = new QuickBarSwap();
        packetCache.Deserialize(result, bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ItemDrop()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x14);
        var expected = new AutoFaker<ItemDrop>().Generate();
        var bytes = Array.Empty<byte>()
            .Append(expected.Window)
            .Concat(BitConverter.GetBytes(expected.Position))
            .Concat(BitConverter.GetBytes(expected.Gold))
            .Append(expected.Count)
            .ToArray();
        var result = new ItemDrop();
        packetCache.Deserialize(result, bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ClickNpc()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x1a);
        var expected = new AutoFaker<ClickNpc>().Generate();
        var bytes = Array.Empty<byte>()
            .Concat(BitConverter.GetBytes(expected.Vid))
            .ToArray();
        var result = new ClickNpc();
        packetCache.Deserialize(result, bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void QuestAnswer()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x1D);
        var expected = new AutoFaker<QuestAnswer>().Generate();
        var bytes = Array.Empty<byte>()
            .Append(expected.Answer)
            .ToArray();
        var result = new QuestAnswer();
        packetCache.Deserialize(result, bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ShopClose()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x32 << 8 | 0x00);
        // var expected = new AutoFaker<ShopClose>().Generate();
        var bytes = Array.Empty<byte>()
            .Append((byte)0x00)
            .ToArray();
        var result = new ShopClose();
        packetCache.Deserialize(result, bytes);

        // result.Should().BeEquivalentTo(expected);
        Assert.Empty(typeof(ShopClose).GetProperties());
    }

    [Fact]
    public void ShopBuy()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x32 << 8 | 0x01);
        var expected = new AutoFaker<ShopBuy>().Generate();
        var bytes = Array.Empty<byte>()
            .Append((byte)0x01) // sub header
            .Append(expected.Count)
            .Append(expected.Position)
            .ToArray();
        var result = new ShopBuy();
        packetCache.Deserialize(result, bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ShopSell()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x32 << 8 | 0x03);
        var expected = new AutoFaker<ShopSell>().Generate();
        var bytes = Array.Empty<byte>()
            .Append((byte)0x03) // sub header
            .Append(expected.Position)
            .Append(expected.Count)
            .ToArray();
        var result = new ShopSell();
        packetCache.Deserialize(result, bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void TargetChange()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x3d);
        var expected = new AutoFaker<TargetChange>().Generate();
        var bytes = Array.Empty<byte>()
            .Concat(BitConverter.GetBytes(expected.TargetVid))
            .ToArray();
        var result = new TargetChange();
        packetCache.Deserialize(result, bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ItemGive()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x53);
        var expected = new AutoFaker<ItemGive>().Generate();
        var bytes = Array.Empty<byte>()
            .Concat(BitConverter.GetBytes(expected.TargetVid))
            .Append(expected.Window)
            .Concat(BitConverter.GetBytes(expected.Position))
            .Append(expected.Count)
            .ToArray();
        var result = new ItemGive();
        packetCache.Deserialize(result, bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Empire()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x5a);
        var expected = new AutoFaker<Empire>().Generate();
        var bytes = Array.Empty<byte>()
            .Append(expected.EmpireId)
            .ToArray();
        var result = new Empire();
        packetCache.Deserialize(result, bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void TokenLogin()
    {
        var packetCache = _packetManager.GetIncomingPacket(0x6d);
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
        var result = new TokenLogin();
        packetCache.Deserialize(result, bytes);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Version()
    {
        var packetCache = _packetManager.GetIncomingPacket(0xf1);
        var expected = new AutoFaker<Version>()
            .RuleFor(x => x.ExecutableName, faker => faker.Lorem.Letter(33))
            .RuleFor(x => x.Timestamp, faker => faker.Lorem.Letter(33))
            .Generate();
        var bytes = Array.Empty<byte>()
            .Concat(Encoding.ASCII.GetBytes(expected.ExecutableName))
            .Concat(Encoding.ASCII.GetBytes(expected.Timestamp))
            .ToArray();
        var result = new Version();
        packetCache.Deserialize(result, bytes);

        result.Should().BeEquivalentTo(expected);
    }
}