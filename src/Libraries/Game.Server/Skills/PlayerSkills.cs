using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuantumCore.API;
using QuantumCore.API.Extensions;
using QuantumCore.API.Game.Skills;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.Types.Players;
using QuantumCore.API.Game.Types.Skills;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets.Skills;
using QuantumCore.Game.Persistence;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Skills;

public class PlayerSkills : IPlayerSkills
{
    private readonly ConcurrentDictionary<ESkill, Skill>
        _skills = new(); //todo: probably no need for concurrent variant

    private readonly ILogger<PlayerSkills> _logger;
    private readonly PlayerEntity _player;
    private readonly IDbPlayerSkillsRepository _repository;
    private readonly ISkillManager _skillManager;
    private readonly SkillsOptions _skillsOptions;

    public const int SkillMaxNum = 255;
    public const int SkillMaxLevel = 40;
    public const int SkillCount = 6;
    public const int JobMaxNum = 4;
    public const int SkillGroupMaxNum = 2;
    public const int MinimumLevel = 5;
    public const int MinimumLevelSubSkills = 10;
    public const int MinimumSkillLevelUpgrade = 17;

    #region Static Skill Data

    private static readonly ESkill[,,] SkillList = new ESkill[JobMaxNum, SkillGroupMaxNum, SkillCount]
    {
        // Warrior
        {
            {
                ESkill.ThreeWayCut, ESkill.SwordSpin, ESkill.BerserkerFury,
                ESkill.AuraOfTheSword, ESkill.Dash, ESkill.Life
            },
            {
                ESkill.Shockwave, ESkill.Bash, ESkill.Stump, ESkill.StrongBody,
                ESkill.SwordStrike, ESkill.SwordOrb
            }
        },
        // Ninja
        {
            {
                ESkill.Ambush, ESkill.FastAttack, ESkill.RollingDagger, ESkill.Stealth,
                ESkill.PoisonousCloud, ESkill.InsidiousPoison
            },
            {
                ESkill.RepetitiveShot, ESkill.ArrowShower, ESkill.FireArrow,
                ESkill.FeatherWalk,
                ESkill.PoisonArrow, ESkill.Spark
            }
        },
        // Sura
        {
            {
                ESkill.FingerStrike, ESkill.DragonSwirl, ESkill.EnchantedBlade, ESkill.Fear,
                ESkill.EnchantedArmor, ESkill.Dispel
            },
            {
                ESkill.DarkStrike, ESkill.FlameStrike, ESkill.FlameSpirit,
                ESkill.DarkProtection, ESkill.SpiritStrike, ESkill.DarkOrb
            }
        },
        // Shaman
        {
            {
                ESkill.FlyingTalisman, ESkill.ShootingDragon, ESkill.DragonRoar,
                ESkill.Blessing, ESkill.Reflect, ESkill.DragonAid
            },
            {
                ESkill.LightningThrow, ESkill.SummonLightning, ESkill.LightningClaw,
                ESkill.Cure, ESkill.Swiftness, ESkill.AttackUp
            }
        }
    };

    private static readonly ImmutableArray<ESkill> PassiveSkillIds =
    [
        ESkill.Leadership,
        ESkill.Combo,
        ESkill.Mining,
        ESkill.LanguageShinsoo,
        ESkill.LanguageChunjo,
        ESkill.LanguageJinno,
        ESkill.Polymorph,
        ESkill.HorseRiding,
        ESkill.HorseSummon,
        ESkill.HorseWildAttack,
        ESkill.HorseCharge,
        ESkill.HorseEscape,
        ESkill.HorseWildAttackRange,
        ESkill.AddHp,
        ESkill.PenetrationResistance
    ];

    #endregion

    public PlayerSkills(ILogger<PlayerSkills> logger, PlayerEntity player, IDbPlayerSkillsRepository repository,
        ISkillManager skillManager, IOptions<SkillsOptions> skillsOptions)
    {
        _logger = logger;
        _player = player;
        _repository = repository;
        _skillManager = skillManager;
        _skillsOptions = skillsOptions.Value;
    }

    public async Task LoadAsync()
    {
        if (_player.Player.SkillGroup > 0)
        {
            AssignDefaultActiveSkills();
        }

        AssignDefaultPassiveSkills();

        var skills = await _repository.GetPlayerSkillsAsync(_player.Player.Id);

        foreach (var skill in skills)
        {
            _skills[skill.SkillId] = skill ?? throw new InvalidOperationException();
        }
    }

    public async Task PersistAsync()
    {
        foreach (var skill in _skills.Values)
        {
            await _repository.SavePlayerSkillAsync(skill);
        }
    }

    public ISkill? this[ESkill skillId] => _skills.TryGetValue(skillId, out var skill) ? skill : null;

    public void SetSkillGroup(byte skillGroup)
    {
        if (skillGroup > SkillGroupMaxNum) return;
        if (_player.GetPoint(EPoint.Level) < MinimumLevel) return;

        // todo: prevent changing skill group in certain situations

        _player.Player.SkillGroup = skillGroup;

        AssignDefaultActiveSkills();

        _player.Connection.Send(new ChangeSkillGroup {SkillGroup = skillGroup});
    }

    public void ClearSkills()
    {
        var points = _player.GetPoint(EPoint.Level) < MinimumLevel
            ? 0
            : (MinimumLevel - 1) + (_player.GetPoint(EPoint.Level) - MinimumLevel) - _player.GetPoint(EPoint.Skill);
        _player.SetPoint(EPoint.Skill, points);

        ResetSkills();
    }

    public void ClearSubSkills()
    {
        var points = _player.GetPoint(EPoint.Level) < MinimumLevelSubSkills
            ? 0
            : (_player.GetPoint(EPoint.Level) - (MinimumLevelSubSkills - 1)) - _player.GetPoint(EPoint.SubSkill);

        _player.SetPoint(EPoint.SubSkill, points);

        ResetSubSkills();
    }

    private void ResetSkills()
    {
        // store subskills in a temporary variable, clear the dictionary and then restore the subskills
        var subSkills = _skills
            .Where(sk => PassiveSkillIds.Contains(sk.Key))
            .ToDictionary(sk => sk.Key, sk => sk.Value);

        _skills.Clear();

        if (_player.Player.SkillGroup > 0)
        {
            AssignDefaultActiveSkills();
        }

        foreach (var subSkill in subSkills)
        {
            _skills[subSkill.Key] = subSkill.Value;
        }

        SendSkillLevelsPacket();
    }

    private void ResetSubSkills()
    {
        // Assign default values to passive skills
        AssignDefaultPassiveSkills();

        SendSkillLevelsPacket();
    }

    public void Reset(ESkill skillId)
    {
        if (skillId >= ESkill.SkillMax)
        {
            return;
        }

        if (!_skills.TryGetValue(skillId, out var skill)) return;

        var level = skill.Level;

        skill.Level = 0;
        skill.MasterType = ESkillMasterType.Normal;
        skill.NextReadTime = 0;

        if (level > MinimumSkillLevelUpgrade)
            level = MinimumSkillLevelUpgrade;

        _player.AddPoint(EPoint.Skill, level);

        SendSkillLevelsPacket();
    }

    public void SetLevel(ESkill skillId, byte level)
    {
        if (skillId >= ESkill.SkillMax)
        {
            return;
        }

        if (!_skills.TryGetValue(skillId, out var skill)) return;

        skill.Level = Math.Min((byte)SkillMaxLevel, level);

        skill.MasterType = level switch
        {
            >= SkillMaxLevel => ESkillMasterType.PerfectMaster,
            >= 30 => ESkillMasterType.GrandMaster,
            >= 20 => ESkillMasterType.Master,
            _ => ESkillMasterType.Normal
        };

        // Reset reads required when new master type is learned
        switch (skill)
        {
            case {Level: 20, ReadsRequired: 0, MasterType: ESkillMasterType.Master}:
            case {Level: 30, ReadsRequired: 0, MasterType: ESkillMasterType.GrandMaster}:
                skill.ReadsRequired = 1;
                break;
        }
    }

    public void SkillUp(ESkill skillId, ESkillLevelMethod method = ESkillLevelMethod.Point)
    {
        if (skillId >= ESkill.SkillMax)
        {
            _logger.LogWarning("Invalid skill id: {SkillId}", skillId);
            return;
        }

        var proto = _skillManager.GetSkill(skillId);
        if (proto == null)
        {
            _logger.LogWarning("Skill not found: {SkillId}", skillId);
            return;
        }

        if (proto.Id >= ESkill.SkillMax)
        {
            _logger.LogWarning("Invalid skill id: {SkillId}", skillId);
            return;
        }

        var skill = _skills.TryGetValue(proto.Id, out var playerSkill) ? playerSkill : null;
        if (skill == null)
        {
            _logger.LogWarning("Skill not found: {SkillId}", skillId);
            return;
        }

        if (!IsLearnableSkill(skillId))
        {
            _player.SendChatInfo("You cannot learn this skill.");
            return;
        }

        if (proto.Type != 0)
        {
            switch (skill.MasterType)
            {
                case ESkillMasterType.GrandMaster:
                    if (method != ESkillLevelMethod.Quest) return;
                    break;
                case ESkillMasterType.PerfectMaster:
                    return;
            }
        }

        switch (method)
        {
            case ESkillLevelMethod.Point when skill.MasterType != ESkillMasterType.Normal:
            case ESkillLevelMethod.Point when (proto.Flags & ESkillFlags.DisableByPointUp) == ESkillFlags.DisableByPointUp:
            case ESkillLevelMethod.Book when proto.Type != 0 && skill.MasterType != ESkillMasterType.Master:
                return;
        }

        if (_player.GetPoint(EPoint.Level) < proto.LevelLimit) return;

        if (proto.PrerequisiteSkillVnum > 0)
        {
            if (skill.MasterType == ESkillMasterType.Normal && GetSkillLevel(proto.Id) < proto.PrerequisiteSkillLevel)
            {
                _player.SendChatInfo("You need to learn the prerequisite skill first.");
                return;
            }
        }

        if (_player.Player.SkillGroup == 0) return;

        if (method == ESkillLevelMethod.Point)
        {
            EPoint idx; // enum

            switch (proto.Type)
            {
                case ESkillCategoryType.PassiveSkills:
                    idx = EPoint.SubSkill;
                    break;
                case ESkillCategoryType.WarriorSkills: // warrior
                case ESkillCategoryType.NinjaSkills: // ninja
                case ESkillCategoryType.SuraSkills: // sura
                case ESkillCategoryType.ShamanSkills: // shaman
                    idx = EPoint.Skill;
                    break;
                case ESkillCategoryType.HorseSkills:
                    idx = EPoint.HorseSkill;
                    break;
                default:
                    _logger.LogWarning("Invalid skill type: {SkillType}", proto.Type);
                    return;
            }

            if ((int)idx == 0) return;

            if (_player.GetPoint(idx) < 1) return;

            _player.AddPoint(idx, -1);
        }

        SetLevel(proto.Id, (byte)(GetSkillLevel(proto.Id) + 1));

        if (proto.Type != ESkillCategoryType.PassiveSkills)
        {
            switch (skill.MasterType)
            {
                case ESkillMasterType.Normal:
                    if (GetSkillLevel(proto.Id) >= MinimumSkillLevelUpgrade)
                    {
                        //todo: implement reset scroll quest flag
                        var random = CoreRandom.GenerateInt32(1, 21 - Math.Min(20, GetSkillLevel(proto.Id)) + 1);
                        if (random == 1)
                        {
                            SetLevel(proto.Id, 20);
                        }
                    }

                    break;
                case ESkillMasterType.Master:
                    if (GetSkillLevel(proto.Id) >= 30)
                    {
                        var random = CoreRandom.GenerateInt32(1, 31 - Math.Min(30, GetSkillLevel(proto.Id)) + 1);
                        if (random == 1)
                        {
                            SetLevel(proto.Id, 30);
                        }
                    }

                    break;
                case ESkillMasterType.GrandMaster:
                    if (GetSkillLevel(proto.Id) >= SkillMaxLevel)
                    {
                        SetLevel(proto.Id, SkillMaxLevel);
                    }

                    break;
            }
        }

        _logger.LogInformation("Skill up: {SkillId} ({Name}) [{Master}] -> {Level}", proto.Id, proto.Name,
            skill.MasterType, GetSkillLevel(proto.Id));

        _player.SendPoints();
        SendSkillLevelsPacket();
    }

    private bool IsLearnableSkill(ESkill skillId)
    {
        var proto = _skillManager.GetSkill(skillId);
        if (proto == null)
        {
            return false;
        }

        if (GetSkillLevel(skillId) >= SkillMaxLevel) return false;

        if (proto.Type == ESkillCategoryType.PassiveSkills)
        {
            return GetSkillLevel(skillId) < proto.MaxLevel;
        }

        if (proto.Type == ESkillCategoryType.HorseSkills)
        {
            return skillId != ESkill.HorseWildAttackRange ||
                   _player.Player.PlayerClass.GetClass()== EPlayerClass.Ninja;
        }

        if (_player.Player.SkillGroup == 0) return false;

        return (int)proto.Type - 1 == (byte)_player.Player.PlayerClass;
    }

    private int GetSkillLevel(ESkill skillId)
    {
        if (skillId >= ESkill.SkillMax) return 0;

        return Math.Min(SkillMaxLevel, _skills.TryGetValue(skillId, out var skill) ? skill.Level : 0);
    }

    public bool CanUse(ESkill skillId)
    {
        if (skillId == 0) return false;

        var skillGroup = _player.Player.SkillGroup;

        if (skillGroup > 0) // if skill group was chosen
        {
            for (var i = 0; i < SkillCount; i++)
            {
                if (SkillList[(int)_player.Player.PlayerClass, skillGroup - 1, i] == skillId)
                {
                    return true;
                }
            }
        }

        // todo: horse riding check

        switch (skillId)
        {
            case ESkill.Leadership:
            case ESkill.Combo:
            case ESkill.Mining:
            case ESkill.LanguageShinsoo:
            case ESkill.LanguageChunjo:
            case ESkill.LanguageJinno:
            case ESkill.Polymorph:
            case ESkill.HorseRiding:
            case ESkill.HorseSummon:
            case ESkill.GuildEye:
            case ESkill.GuildBlood:
            case ESkill.GuildBless:
            case ESkill.GuildSeonghwi:
            case ESkill.GuildAcceleration:
            case ESkill.GuildBunno:
            case ESkill.GuildJumun:
            case ESkill.GuildTeleport:
            case ESkill.GuildDoor:
                return true;
        }

        return false;
    }

    public void Send()
    {
        SendSkillLevelsPacket();
    }

    public bool LearnSkillByBook(ESkill skillId)
    {
        var proto = _skillManager.GetSkill(skillId);
        if (proto == null)
        {
            return false;
        }

        if (!IsLearnableSkill(skillId))
        {
            _player.SendChatInfo("You cannot learn this skill.");
            return false;
        }

        if (_player.GetPoint(EPoint.Experience) < _skillsOptions.SkillBookNeededExperience)
        {
            _player.SendChatInfo("Not enough experience.");
            return false;
        }

        var skill = _skills.TryGetValue(skillId, out var playerSkill) ? playerSkill : null;
        if (skill == null)
        {
            _logger.LogWarning("Skill not found: {SkillId}", skillId);
            return false;
        }

        if (proto.Type != 0)
        {
            if (skill.MasterType != ESkillMasterType.Master)
            {
                _player.SendChatInfo("You cannot learn this skill.");
                return false;
            }
        }

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        if (currentTime < skill.NextReadTime)
        {
            _player.SendChatInfo(
                $"You cannot read this skill book yet. {skill.NextReadTime - currentTime} seconds to wait.");
            return false;
        }

        _player.AddPoint(EPoint.Experience, -_skillsOptions.SkillBookNeededExperience);

        var previousLevel = skill.Level;

        var readSuccess = CoreRandom.GenerateInt32(1, 3) == 1;

        if (readSuccess)
        {
            if (skill.ReadsRequired - 1 == 0)
            {
                SkillUp(skillId, ESkillLevelMethod.Book);
                skill.ReadsRequired = skill.Level switch
                {
                    21 => 2,
                    22 => 3,
                    23 => 4,
                    24 => 5,
                    25 => 6,
                    26 => 7,
                    27 => 8,
                    28 => 9,
                    29 => 10,
                    30 => 1,
                    _ => 0
                };
            }
            else
            {
                skill.ReadsRequired = Math.Max(1, skill.ReadsRequired - 1);
            }
        }

        if (previousLevel != skill.Level)
        {
            _player.SendChatInfo($"You have learned the skill.");
        }
        else
        {
            _player.SendChatInfo(readSuccess
                ? $"You have learned the skill book. {skill.ReadsRequired} books left."
                : "Failed to read the skill book.");
        }

        return true;
    }

    public void SetSkillNextReadTime(ESkill skillId, int time)
    {
        if (skillId >= ESkill.SkillMax)
        {
            return;
        }

        if (!_skills.TryGetValue(skillId, out var skill)) return;

        skill.NextReadTime = time;
    }

    private void SendSkillLevelsPacket()
    {
        var levels = new SkillLevels();
        for (var i = 0; i < SkillMaxNum; i++)
        {
            levels.Skills[i] = new PlayerSkill {Level = 0, MasterType = (byte)ESkillMasterType.Normal, NextReadTime = 0};
        }

        for (var i = 0; i < _skills.Count; i++)
        {
            var skill = _skills.ElementAt(i);

            levels.Skills[(uint)skill.Key] = new PlayerSkill
            {
                Level = skill.Value.Level,
                MasterType = (byte)skill.Value.MasterType,
                NextReadTime = skill.Value.NextReadTime
            };
        }

        _player.Connection.Send(levels);
    }

    private void AssignDefaultActiveSkills()
    {
        for (var i = 0; i < SkillCount; i++)
        {
            var skill = SkillList[(int)_player.Player.PlayerClass, _player.Player.SkillGroup - 1, i];
            if (skill == 0) continue;

            _skills[skill] = new Skill
            {
                Level = 0,
                MasterType = ESkillMasterType.Normal,
                NextReadTime = 0,
                SkillId = skill,
                PlayerId = _player.Player.Id,
            };
        }
    }

    private void AssignDefaultPassiveSkills()
    {
        foreach (var skill in PassiveSkillIds)
        {
            _skills[skill] = new Skill
            {
                Level = 0,
                MasterType = ESkillMasterType.Normal,
                NextReadTime = 0,
                SkillId = skill,
                PlayerId = _player.Player.Id,
            };
        }
    }
}
