using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using NSubstitute;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Game;

namespace Game.Tests;

public class MobProtoTests
{
    private readonly IMonsterManager _monsterManager;

    public MobProtoTests()
    {
        _monsterManager = new ServiceCollection()
            .AddSingleton<IMonsterManager, MonsterManager>()
            .AddLogging()
            .AddSingleton(_ =>
            {
                var mock = Substitute.For<IFileProvider>();
                mock.GetFileInfo(Arg.Any<string>()).ReturnsForAnyArgs(call =>
                    new PhysicalFileInfo(new FileInfo(Path.Combine("Fixtures", call.Arg<string>()))));
                return mock;
            })
            .BuildServiceProvider()
            .GetRequiredService<IMonsterManager>();
    }

    [Fact]
    public async Task CanRead()
    {
        await _monsterManager.LoadAsync();
        var monsters = _monsterManager.GetMonsters();
        // map to another type so we don't include any library properties
        new MonsterData
        {
            Id = monsters[0].Id,
            Name = monsters[0].Name,
            TranslatedName = monsters[0].TranslatedName,
            Type = monsters[0].Type,
            Rank = monsters[0].Rank,
            BattleType = monsters[0].BattleType,
            Level = monsters[0].Level,
            Size = monsters[0].Size,
            MinGold = monsters[0].MinGold,
            MaxGold = monsters[0].MaxGold,
            Experience = monsters[0].Experience,
            Hp = monsters[0].Hp,
            RegenDelay = monsters[0].RegenDelay,
            RegenPercentage = monsters[0].RegenPercentage,
            Defence = monsters[0].Defence,
            AiFlag = monsters[0].AiFlag,
            RaceFlag = monsters[0].RaceFlag,
            ImmuneFlag = monsters[0].ImmuneFlag,
            St = monsters[0].St,
            Dx = monsters[0].Dx,
            Ht = monsters[0].Ht,
            Iq = monsters[0].Iq,
            DamageRange = monsters[0].DamageRange,
            AttackSpeed = monsters[0].AttackSpeed,
            MoveSpeed = monsters[0].MoveSpeed,
            AggressivePct = monsters[0].AggressivePct,
            AggressiveSight = monsters[0].AggressiveSight,
            AttackRange = monsters[0].AttackRange,
            Enchantments = monsters[0].Enchantments,
            Resists = monsters[0].Resists,
            ResurrectionId = monsters[0].ResurrectionId,
            DropItemId = monsters[0].DropItemId,
            MountCapacity = monsters[0].MountCapacity,
            OnClickType = monsters[0].OnClickType,
            Empire = monsters[0].Empire,
            Folder = monsters[0].Folder,
            DamageMultiply = monsters[0].DamageMultiply,
            SummonId = monsters[0].SummonId,
            DrainSp = monsters[0].DrainSp,
            MonsterColor = monsters[0].MonsterColor,
            PolymorphItemId = monsters[0].PolymorphItemId,
            Skills = monsters[0].Skills.Select(x => new MonsterSkillData { Id = x.Id, Level = x.Level }).ToList(),
            BerserkPoint = monsters[0].BerserkPoint,
            StoneSkinPoint = monsters[0].StoneSkinPoint,
            GodSpeedPoint = monsters[0].GodSpeedPoint,
            DeathBlowPoint = monsters[0].DeathBlowPoint,
            RevivePoint = monsters[0].RevivePoint,
        }.Should().BeEquivalentTo(new MonsterData
        {
            Id = 101,
            Name = "??",
            TranslatedName = "Wild Dog",
            Type = (byte)EEntityType.Monster,
            Rank = (byte)EMonsterLevel.Pawn,
            BattleType = (byte)EBattleType.Melee,
            Level = 1,
            Size = 0,
            // gold is not saved in client side mob proto (at least not by dump_proto)
            MinGold = 0,
            MaxGold = 0,
            Experience = 15,
            Hp = 126,
            RegenDelay = 6,
            RegenPercentage = 7,
            Defence = 4,
            AiFlag = 0,
            RaceFlag = (uint)ERaceFlag.Animal,
            ImmuneFlag = 0,
            St = 3,
            Dx = 6,
            Ht = 5,
            Iq = 2,
            DamageRange = [20, 24],
            AttackSpeed = 100,
            MoveSpeed = 100,
            AggressivePct = 0,
            AggressiveSight = 2000,
            AttackRange = 175,
            Enchantments =
            [
                0, 0, 0, 0,
                0, 0
            ],
            Resists =
            [
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0
            ],
            ResurrectionId = 0,
            DropItemId = 0,
            MountCapacity = 0,
            OnClickType = 0,
            Empire = 0,
            Folder = "stray_dog",
            DamageMultiply = 1,
            SummonId = 0,
            DrainSp = 0,
            MonsterColor = 0,
            // PolymorphItemId is not saved in client side mob proto (at least not by dump_proto)
            PolymorphItemId = 0,
            Skills =
            [
                new MonsterSkillData { Id = 0, Level = 0 },
                new MonsterSkillData { Id = 0, Level = 0 },
                new MonsterSkillData { Id = 0, Level = 0 },
                new MonsterSkillData { Id = 0, Level = 0 },
                new MonsterSkillData { Id = 0, Level = 0 }
            ],
            BerserkPoint = 0,
            StoneSkinPoint = 0,
            GodSpeedPoint = 0,
            DeathBlowPoint = 0,
            RevivePoint = 0
        });
    }
}
