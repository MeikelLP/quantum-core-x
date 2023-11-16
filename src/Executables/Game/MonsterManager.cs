using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.Core.Types;

namespace QuantumCore.Game
{
    /// <summary>
    /// Manage all static data related to monster
    /// </summary>
    public class MonsterManager : IMonsterManager
    {
        private readonly ILogger<MonsterManager> _logger;
        private MobProto? _proto;

        public MonsterManager(ILogger<MonsterManager> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Try to load mob_proto file
        /// </summary>
        public Task LoadAsync(CancellationToken token = default)
        {
            _logger.LogInformation("Loading mob_proto");
            _proto = MobProto.FromFile("data/mob_proto");
            _logger.LogDebug("Loaded {Count} monsters", _proto.Content.Data.Monsters.Count);

            return Task.CompletedTask;
        }

        public MonsterData? GetMonster(uint id)
        {
            var proto = _proto?.Content.Data.Monsters.FirstOrDefault(monster => monster.Id == id);

            if (proto is not null)
            {
                return ToMonsterData(proto);
            }

            return null;
        }

        public List<MonsterData> GetMonsters()
        {
            return _proto?.Content.Data.Monsters.Select(ToMonsterData).ToList() ?? new List<MonsterData>();
        }

        private static MonsterData ToMonsterData(MobProto.Monster proto)
        {
            return new MonsterData {
                Defence = proto.Defence,
                Dx = proto.Dx,
                Empire = proto.Empire,
                Enchantments = proto.Enchantments,
                Experience = proto.Experience,
                Folder = proto.Folder,
                Hp = proto.Hp,
                Ht = proto.Ht,
                Id = proto.Id,
                Iq = proto.Iq,
                Level = proto.Level,
                Name = proto.Name,
                Rank = proto.Rank,
                Resists = proto.Resists,
                Size = proto.Size,
                Skills = proto.Skills.Select(x => new MonsterSkillData { Id = x.Id, Level = x.Level }).ToList(),
                St = proto.St,
                Type = proto.Type,
                AggressivePct = proto.AggressivePct,
                AggressiveSight = proto.AggressiveSight,
                AiFlag = proto.AiFlag,
                AttackRange = proto.AttackRange,
                AttackSpeed = proto.AttackSpeed,
                BattleType = proto.BattleType,
                BerserkPoint = proto.BerserkPoint,
                DamageMultiply = proto.DamageMultiply,
                DamageRange = proto.DamageRange,
                DrainSp = proto.DrainSp,
                ImmuneFlag = proto.ImmuneFlag,
                MaxGold = proto.MaxGold,
                MinGold = proto.MinGold,
                MonsterColor = proto.MonsterColor,
                MountCapacity = proto.MountCapacity,
                MoveSpeed = proto.MoveSpeed,
                RaceFlag = proto.RaceFlag,
                RegenDelay = proto.RegenDelay,
                RegenPercentage = proto.RegenPercentage,
                ResurrectionId = proto.ResurrectionId,
                RevivePoint = proto.RevivePoint,
                SummonId = proto.SummonId,
                TranslatedName = proto.TranslatedName,
                DeathBlowPoint = proto.DeathBlowPoint,
                DropItemId = proto.DropItemId,
                GodSpeedPoint = proto.GodSpeedPoint,
                OnClickType = proto.OnClickType,
                PolymorphItemId = proto.PolymorphItemId,
                StoneSkinPoint = proto.StoneSkinPoint
            };
        }
    }
}
