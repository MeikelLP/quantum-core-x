using System.Text;
using AutoBogus;
using Bogus;
using Core.Tests.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuantumCore.Extensions;
using QuantumCore.Game.Packets;
using QuantumCore.Game.Packets.General;
using QuantumCore.Game.Packets.Quest;
using QuantumCore.Game.Packets.QuickBar;
using QuantumCore.Game.Packets.Shop;
using QuantumCore.Networking;
using Weikio.PluginFramework.Catalogs;
using Xunit;
using Xunit.Abstractions;

namespace Core.Tests;

public class OutgoingPacketTests
{
    private readonly IPacketSerializer _serializer;

    public OutgoingPacketTests(ITestOutputHelper testOutputHelper)
    {
        var services = new ServiceCollection()
            .AddCoreServices(new EmptyPluginCatalog(), new ConfigurationBuilder().Build())
            .AddSingleton<IPacketSerializer, DefaultPacketSerializer>()
            .AddQuantumCoreTestLogger(testOutputHelper)
            .BuildServiceProvider();
        _serializer = services.GetRequiredService<IPacketSerializer>();
    }

    [Fact]
    public void NullReturnsArrayWithDefaultValues()
    {
        Assert.Throws<NullReferenceException>(() => _serializer.Serialize((ServerStatusPacket)null!));
    }

    [Fact]
    public void NullableStringDoesNotThrow()
    {
        _serializer.Serialize(new CreateCharacter());
        Assert.True(true);
    }

    [Fact]
    public void SpawnCharacter()
    {
        var obj = new AutoFaker<SpawnCharacter>()
            .RuleFor(x => x.Affects, faker => new[] {faker.Random.UInt(), faker.Random.UInt()})
            .Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x01}
                .Concat(BitConverter.GetBytes(obj.Vid))
                .Concat(BitConverter.GetBytes(obj.Angle))
                .Concat(BitConverter.GetBytes(obj.PositionX))
                .Concat(BitConverter.GetBytes(obj.PositionY))
                .Concat(BitConverter.GetBytes(obj.PositionZ))
                .Append(obj.CharacterType)
                .Concat(BitConverter.GetBytes(obj.Class))
                .Append(obj.MoveSpeed)
                .Append(obj.AttackSpeed)
                .Append(obj.State)
                .Concat(obj.Affects.SelectMany(BitConverter.GetBytes))
        );
    }

    [Fact]
    public void RemoveCharacter()
    {
        var obj = new AutoFaker<RemoveCharacter>().Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x02}
                .Concat(BitConverter.GetBytes(obj.Vid))
        );
    }

    [Fact]
    public void CharacterMoveOut()
    {
        var obj = new AutoFaker<CharacterMoveOut>().Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x03}
                .Append(obj.MovementType)
                .Append(obj.Argument)
                .Append(obj.Rotation)
                .Concat(BitConverter.GetBytes(obj.Vid))
                .Concat(BitConverter.GetBytes(obj.PositionX))
                .Concat(BitConverter.GetBytes(obj.PositionY))
                .Concat(BitConverter.GetBytes(obj.Time))
                .Concat(BitConverter.GetBytes(obj.Duration))
                .Append((byte)0x00)
        );
    }

    [Fact]
    public void ChatOutcoming()
    {
        var obj = new AutoFaker<ChatOutcoming>().Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x04}
                .Concat(BitConverter.GetBytes((short)obj.GetSize()))
                .Append((byte)obj.MessageType)
                .Concat(BitConverter.GetBytes(obj.Vid))
                .Append((byte)obj.Empire)
                .Concat(Encoding.ASCII.GetBytes(obj.Message))
                .Append((byte)0)
        );
    }

    [Fact]
    public void CreateCharacterSuccess()
    {
        var charFaker = new Faker<Character>()
            .RuleFor(x => x.Name, faker => faker.Lorem.Letter(24) + '\0');
        var obj = new AutoFaker<CreateCharacterSuccess>()
            .RuleFor(x => x.Character, _ => charFaker.Generate())
            .Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x08}
                .Append(obj.Slot)
                .Concat(BitConverter.GetBytes(obj.Character.Id))
                .Concat(Encoding.ASCII.GetBytes(obj.Character.Name))
                .Append((byte)0) // null byte for end of string
                .Append(obj.Character.Level)
                .Concat(BitConverter.GetBytes(obj.Character.Playtime))
                .Append(obj.Character.St)
                .Append(obj.Character.Ht)
                .Append(obj.Character.Dx)
                .Append(obj.Character.Iq)
                .Concat(BitConverter.GetBytes(obj.Character.BodyPart))
                .Append(obj.Character.NameChange)
                .Concat(BitConverter.GetBytes(obj.Character.HairPort))
                .Concat(BitConverter.GetBytes(obj.Character.Unknown))
                .Concat(BitConverter.GetBytes(obj.Character.PositionX))
                .Concat(BitConverter.GetBytes(obj.Character.PositionY))
                .Concat(BitConverter.GetBytes(obj.Character.Ip))
                .Concat(BitConverter.GetBytes(obj.Character.Port))
                .Append(obj.Character.SkillGroup)
        );
    }

    [Fact]
    public void CreateCharacterFailure()
    {
        var obj = new AutoFaker<CreateCharacterFailure>().Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x09}
                .Append(obj.Error)
        );
    }

    [Fact]
    public void DeleteCharacterSuccess()
    {
        var obj = new AutoFaker<DeleteCharacterSuccess>().Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x0A}
                .Append(obj.Slot)
        );
    }

    [Fact]
    public void DeleteCharacterFail()
    {
        var obj = new AutoFaker<DeleteCharacterFail>().Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x0B}
        );
    }

    [Fact]
    public void CharacterDead()
    {
        var obj = new AutoFaker<CharacterDead>().Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x0e}
                .Concat(BitConverter.GetBytes(obj.Vid)));
    }

    [Fact]
    public void CharacterPoints()
    {
        var obj = new AutoFaker<CharacterPoints>()
            .RuleFor(x => x.Points,
                faker => { return Enumerable.Range(0, 255).Select(_ => faker.Random.UInt()).ToArray(); })
            .Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x10}
                .Concat(obj.Points.SelectMany(BitConverter.GetBytes))
        );
    }

    [Fact]
    public void CharacterUpdate()
    {
        var obj = new AutoFaker<CharacterUpdate>()
            .RuleFor(x => x.Parts,
                faker => new[]
                {
                    faker.Random.UShort(), faker.Random.UShort(), faker.Random.UShort(), faker.Random.UShort()
                })
            .RuleFor(x => x.Affects, faker => new[] {faker.Random.UInt(), faker.Random.UInt()})
            .Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x13}
                .Concat(BitConverter.GetBytes(obj.Vid))
                .Concat(obj.Parts.SelectMany(BitConverter.GetBytes))
                .Append(obj.MoveSpeed)
                .Append(obj.AttackSpeed)
                .Append(obj.State)
                .Concat(obj.Affects.SelectMany(BitConverter.GetBytes))
                .Concat(BitConverter.GetBytes(obj.GuildId))
                .Concat(BitConverter.GetBytes(obj.RankPoints))
                .Append(obj.PkMode)
                .Concat(BitConverter.GetBytes(obj.MountVnum))
                .ToArray()
        );
    }

    [Fact]
    public void SetItem()
    {
        var itemBonusFaker = new AutoFaker<ItemBonus>();
        var obj = new AutoFaker<SetItem>()
            .RuleFor(x => x.Sockets, faker => new[] {faker.Random.UInt(), faker.Random.UInt(), faker.Random.UInt()})
            .RuleFor(x => x.Bonuses,
                _ => new[]
                {
                    itemBonusFaker.Generate(), itemBonusFaker.Generate(), itemBonusFaker.Generate(),
                    itemBonusFaker.Generate(), itemBonusFaker.Generate(), itemBonusFaker.Generate(),
                    itemBonusFaker.Generate(),
                })
            .Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x15}
                .Append(obj.Window)
                .Concat(BitConverter.GetBytes(obj.Position))
                .Concat(BitConverter.GetBytes(obj.ItemId))
                .Append(obj.Count)
                .Concat(BitConverter.GetBytes(obj.Flags))
                .Concat(BitConverter.GetBytes(obj.AnitFlags))
                .Concat(BitConverter.GetBytes(obj.Highlight))
                .Concat(obj.Sockets.SelectMany(BitConverter.GetBytes))
                .Concat(obj.Bonuses.SelectMany(bonus =>
                {
                    return new[] {bonus.BonusId}.Concat(BitConverter.GetBytes(bonus.Value));
                }))
        );
    }

    [Fact]
    public void GroundItemAdd()
    {
        var obj = new AutoFaker<GroundItemAdd>().Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x1A}
                .Concat(BitConverter.GetBytes(obj.PositionX))
                .Concat(BitConverter.GetBytes(obj.PositionY))
                .Concat(BitConverter.GetBytes(obj.PositionZ))
                .Concat(BitConverter.GetBytes(obj.Vid))
                .Concat(BitConverter.GetBytes(obj.ItemId))
        );
    }

    [Fact]
    public void GroundItemRemove()
    {
        var obj = new AutoFaker<GroundItemRemove>().Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x1B}
                .Concat(BitConverter.GetBytes(obj.Vid))
        );
    }

    [Fact]
    public void QuickBarAddOut()
    {
        var obj = new AutoFaker<QuickBarAddOut>().Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x1C}
                .Append(obj.Position)
                .Append(obj.Slot.Type)
                .Append(obj.Slot.Position)
        );
    }

    [Fact]
    public void QuickBarRemoveOut()
    {
        var obj = new AutoFaker<QuickBarRemoveOut>().Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x1D}
                .Append(obj.Position)
        );
    }

    [Fact]
    public void QuickBarSwapOut()
    {
        var obj = new AutoFaker<QuickBarSwapOut>().Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x1E}
                .Append(obj.Position1)
                .Append(obj.Position2)
        );
    }

    [Fact]
    public void Characters()
    {
        var characterFaker = new AutoFaker<Character>()
            .RuleFor(x => x.Name, faker => faker.Lorem.Letter(24) + '\0');
        var obj = new AutoFaker<Characters>()
            .RuleFor(x => x.GuildIds,
                faker => new[] {faker.Random.UInt(), faker.Random.UInt(), faker.Random.UInt(), faker.Random.UInt()})
            .RuleFor(x => x.GuildNames,
                faker => new[]
                {
                    faker.Lorem.Letter(12) + '\0', faker.Lorem.Letter(12) + '\0', faker.Lorem.Letter(12) + '\0',
                    faker.Lorem.Letter(12) + '\0'
                })
            .RuleFor(x => x.CharacterList, _ =>
            {
                return new[]
                {
                    characterFaker.Generate(), characterFaker.Generate(), characterFaker.Generate(),
                    characterFaker.Generate()
                };
            })
            .Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x20}
                .Concat(obj.CharacterList.SelectMany(c => Array.Empty<byte>()
                    .Concat(BitConverter.GetBytes(c.Id))
                    .Concat(Encoding.ASCII.GetBytes(c.Name))
                    .Append((byte)c.Class)
                    .Append(c.Level)
                    .Concat(BitConverter.GetBytes(c.Playtime))
                    .Append(c.St)
                    .Append(c.Ht)
                    .Append(c.Dx)
                    .Append(c.Iq)
                    .Concat(BitConverter.GetBytes(c.BodyPart))
                    .Append(c.NameChange)
                    .Concat(BitConverter.GetBytes(c.HairPort))
                    .Concat(BitConverter.GetBytes(c.Unknown))
                    .Concat(BitConverter.GetBytes(c.PositionX))
                    .Concat(BitConverter.GetBytes(c.PositionY))
                    .Concat(BitConverter.GetBytes(c.Ip))
                    .Concat(BitConverter.GetBytes(c.Port))
                    .Append(c.SkillGroup)))
                .Concat(obj.GuildIds.SelectMany(BitConverter.GetBytes))
                .Concat(obj.GuildNames.SelectMany(Encoding.ASCII.GetBytes))
                .Concat(BitConverter.GetBytes(obj.Unknown1))
                .Concat(BitConverter.GetBytes(obj.Unknown2))
        );
    }

    [Fact]
    public void ShopOpen()
    {
        var itemBonusFaker = new AutoFaker<ItemBonus>();
        var shopItemFaker = new AutoFaker<ShopItem>()
            .RuleFor(x => x.Sockets, faker => new[] {faker.Random.UInt(), faker.Random.UInt(), faker.Random.UInt()})
            .RuleFor(x => x.Bonuses,
                _ => new[]
                {
                    itemBonusFaker.Generate(), itemBonusFaker.Generate(), itemBonusFaker.Generate(),
                    itemBonusFaker.Generate(), itemBonusFaker.Generate(), itemBonusFaker.Generate(),
                    itemBonusFaker.Generate()
                });
        var obj = new AutoFaker<ShopOpen>()
            .RuleFor(x => x.Items, _ => Enumerable.Range(0, 40).Select(_ => shopItemFaker.Generate()).ToArray())
            .Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x26}
                .Concat(BitConverter.GetBytes(obj.GetSize()))
                .Append((byte)0x00)
                .Concat(BitConverter.GetBytes(obj.Vid))
                .Concat(obj.Items.SelectMany(item => Array.Empty<byte>()
                    .Concat(BitConverter.GetBytes(item.ItemId))
                    .Concat(BitConverter.GetBytes(item.Price))
                    .Append(item.Count)
                    .Append(item.Position)
                    .Concat(item.Sockets.SelectMany(BitConverter.GetBytes))
                    .Concat(item.Bonuses.SelectMany(bonus => Array.Empty<byte>()
                        .Append(bonus.BonusId)
                        .Concat(BitConverter.GetBytes(bonus.Value))))
                ))
        );
    }

    [Fact]
    public void ShopNotEnoughMoney()
    {
        var obj = new AutoFaker<ShopNotEnoughMoney>().Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x26}
                .Append((byte)0x05)
                .Concat(BitConverter.GetBytes(obj.Size))
        );
    }

    [Fact]
    public void ShopNoSpaceLeft()
    {
        var obj = new AutoFaker<ShopNoSpaceLeft>().Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x26}
                .Append((byte)0x07)
        );
    }

    [Fact]
    public void QuestScript()
    {
        var obj = new AutoFaker<QuestScript>().Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x2D}
                .Concat(BitConverter.GetBytes(obj.GetSize()))
                .Append(obj.Skin)
                .Concat(BitConverter.GetBytes(obj.SourceSize))
                .Concat(Encoding.ASCII.GetBytes(obj.Source))
                .Append((byte)0) // null byte for end of string
        );
    }

    [Fact]
    public void SetTarget()
    {
        var obj = new AutoFaker<SetTarget>().Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x3f}
                .Concat(BitConverter.GetBytes(obj.TargetVid))
                .Append(obj.Percentage)
        );
    }

    [Fact]
    public void Warp()
    {
        var obj = new AutoFaker<Warp>().Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x41}
                .Concat(BitConverter.GetBytes(obj.PositionX))
                .Concat(BitConverter.GetBytes(obj.PositionY))
                .Concat(BitConverter.GetBytes(obj.ServerAddress))
                .Concat(BitConverter.GetBytes(obj.ServerPort))
        );
    }

    [Fact]
    public void GameTime()
    {
        var obj = new AutoFaker<GameTime>().Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x6a}
                .Concat(BitConverter.GetBytes(obj.Time))
        );
    }

    [Fact]
    public void CharacterDetails()
    {
        var obj = new AutoFaker<CharacterDetails>()
            .RuleFor(x => x.Name, faker => faker.Lorem.Letter(24) + '\0')
            .Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x71}
                .Concat(BitConverter.GetBytes(obj.Vid))
                .Concat(BitConverter.GetBytes(obj.Class))
                .Concat(Encoding.ASCII.GetBytes(obj.Name))
                .Concat(BitConverter.GetBytes(obj.PositionX))
                .Concat(BitConverter.GetBytes(obj.PositionY))
                .Concat(BitConverter.GetBytes(obj.PositionZ))
                .Append((byte)obj.Empire)
                .Append(obj.SkillGroup)
        );
    }

    [Fact]
    public void Channel()
    {
        var obj = new AutoFaker<Channel>().Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x79}
                .Append(obj.ChannelNo)
        );
    }

    [Fact]
    public void DamageInfo()
    {
        var obj = new AutoFaker<DamageInfo>().Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x87}
                .Concat(BitConverter.GetBytes(obj.Vid))
                .Append(obj.DamageFlags)
                .Concat(BitConverter.GetBytes(obj.Damage))
        );
    }

    [Fact]
    public void CharacterInfo()
    {
        var obj = new AutoFaker<CharacterInfo>()
            .RuleFor(x => x.Name, faker => faker.Lorem.Letter(24) + '\0')
            .RuleFor(x => x.Parts,
                faker => new[]
                {
                    faker.Random.UShort(), faker.Random.UShort(), faker.Random.UShort(), faker.Random.UShort()
                })
            .Generate();
        var bytes = _serializer.Serialize(obj);

        bytes.Should().Equal(
            new byte[] {0x88}
                .Concat(BitConverter.GetBytes(obj.Vid))
                .Concat(Encoding.ASCII.GetBytes(obj.Name))
                .Concat(obj.Parts.SelectMany(BitConverter.GetBytes))
                .Append((byte)obj.Empire)
                .Concat(BitConverter.GetBytes(obj.GuildId))
                .Concat(BitConverter.GetBytes(obj.Level))
                .Concat(BitConverter.GetBytes(obj.RankPoints))
                .Append(obj.PkMode)
                .Concat(BitConverter.GetBytes(obj.MountVnum))
        );
    }
}
