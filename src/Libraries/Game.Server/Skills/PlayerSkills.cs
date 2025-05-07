using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuantumCore.API;
using QuantumCore.API.Game.Skills;
using QuantumCore.API.Game.Types;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets.Skills;
using QuantumCore.Game.Persistence;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Skills;

public class PlayerSkills : IPlayerSkills
{
    private readonly ConcurrentDictionary<ESkillIndexes, Skill>
        _skills = new(); //todo: probably no need for concurrent variant

    private readonly ILogger<PlayerSkills> _logger;
    private readonly PlayerEntity _player;
    private readonly IDbPlayerSkillsRepository _repository;
    private readonly ISkillManager _skillManager;
    private readonly SkillsOptions _skillsOptions;

    private const int SkillMaxNum = 255;
    private const int SkillMaxLevel = 40;
    private const int SkillCount = 6;
    private const int JobMaxNum = 4;
    private const int SkillGroupMaxNum = 2;
    private const int MinimumLevel = 5;
    private const int MinimumLevelSubSkills = 10;
    private const int MinimumSkillLevelUpgrade = 17;

    #region Static Skill Data

    private static readonly ESkillIndexes[,,] SkillList = new ESkillIndexes[JobMaxNum, SkillGroupMaxNum, SkillCount]
    {
        // Warrior
        {
            {
                ESkillIndexes.ThreeWayCut, ESkillIndexes.SwordSpin, ESkillIndexes.BerserkerFury,
                ESkillIndexes.AuraOfTheSword, ESkillIndexes.Dash, ESkillIndexes.Life
            },
            {
                ESkillIndexes.Shockwave, ESkillIndexes.Bash, ESkillIndexes.Stump, ESkillIndexes.StrongBody,
                ESkillIndexes.SwordStrike, ESkillIndexes.SwordOrb
            }
        },
        // Ninja
        {
            {
                ESkillIndexes.Ambush, ESkillIndexes.FastAttack, ESkillIndexes.RollingDagger, ESkillIndexes.Stealth,
                ESkillIndexes.PoisonousCloud, ESkillIndexes.InsidiousPoison
            },
            {
                ESkillIndexes.RepetitiveShot, ESkillIndexes.ArrowShower, ESkillIndexes.FireArrow,
                ESkillIndexes.FeatherWalk,
                ESkillIndexes.PoisonArrow, ESkillIndexes.Spark
            }
        },
        // Sura
        {
            {
                ESkillIndexes.FingerStrike, ESkillIndexes.DragonSwirl, ESkillIndexes.EnchantedBlade, ESkillIndexes.Fear,
                ESkillIndexes.EnchantedArmor, ESkillIndexes.Dispel
            },
            {
                ESkillIndexes.DarkStrike, ESkillIndexes.FlameStrike, ESkillIndexes.FlameSpirit,
                ESkillIndexes.DarkProtection, ESkillIndexes.SpiritStrike, ESkillIndexes.DarkOrb
            }
        },
        // Shaman
        {
            {
                ESkillIndexes.FlyingTalisman, ESkillIndexes.ShootingDragon, ESkillIndexes.DragonRoar,
                ESkillIndexes.Blessing, ESkillIndexes.Reflect, ESkillIndexes.DragonAid
            },
            {
                ESkillIndexes.LightningThrow, ESkillIndexes.SummonLightning, ESkillIndexes.LightningClaw,
                ESkillIndexes.Cure, ESkillIndexes.Swiftness, ESkillIndexes.AttackUp
            }
        }
    };

    private static readonly ImmutableArray<ESkillIndexes> PassiveSkillIds =
    [
        ESkillIndexes.Leadership,
        ESkillIndexes.Combo,
        ESkillIndexes.Mining,
        ESkillIndexes.LanguageShinsoo,
        ESkillIndexes.LanguageChunjo,
        ESkillIndexes.LanguageJinno,
        ESkillIndexes.Polymorph,
        ESkillIndexes.HorseRiding,
        ESkillIndexes.HorseSummon,
        ESkillIndexes.HoseWildAttack,
        ESkillIndexes.HorseCharge,
        ESkillIndexes.HorseEscape,
        ESkillIndexes.HorseWildAttackRange,
        ESkillIndexes.AddHp,
        ESkillIndexes.PenetrationResistance
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

    public ISKill? this[ESkillIndexes skillId] => _skills.TryGetValue(skillId, out var skill) ? skill : null;

    public void SetSkillGroup(byte skillGroup)
    {
        if (skillGroup > SkillGroupMaxNum) return;
        if (_player.GetPoint(EPoints.Level) < MinimumLevel) return;

        // todo: prevent changing skill group in certain situations

        _player.Player.SkillGroup = skillGroup;

        AssignDefaultActiveSkills();

        _player.Connection.Send(new ChangeSkillGroup {SkillGroup = skillGroup});
    }

    public void ClearSkills()
    {
        var points = _player.GetPoint(EPoints.Level) < MinimumLevel
            ? 0
            : (MinimumLevel - 1) + (_player.GetPoint(EPoints.Level) - MinimumLevel) - _player.GetPoint(EPoints.Skill);
        _player.SetPoint(EPoints.Skill, points);

        ResetSkills();
    }

    public void ClearSubSkills()
    {
        var points = _player.GetPoint(EPoints.Level) < MinimumLevelSubSkills
            ? 0
            : (_player.GetPoint(EPoints.Level) - (MinimumLevelSubSkills - 1)) - _player.GetPoint(EPoints.SubSkill);

        _player.SetPoint(EPoints.SubSkill, points);

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

    public void Reset(ESkillIndexes skillId)
    {
        if (skillId >= ESkillIndexes.SkillMax)
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

        _player.AddPoint(EPoints.Skill, level);

        SendSkillLevelsPacket();
    }

    public void SetLevel(ESkillIndexes skillId, byte level)
    {
        if (skillId >= ESkillIndexes.SkillMax)
        {
            return;
        }

        if (!_skills.TryGetValue(skillId, out var skill)) return;

        skill.Level = Math.Min((byte)40, level);

        skill.MasterType = level switch
        {
            >= 40 => ESkillMasterType.PerfectMaster,
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

    public void SkillUp(ESkillIndexes skillId, ESkillLevelMethod method = ESkillLevelMethod.Point)
    {
        if (skillId >= ESkillIndexes.SkillMax)
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

        if (proto.Id >= ESkillIndexes.SkillMax)
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
            case ESkillLevelMethod.Point when (proto.Flag & ESkillFlag.DisableByPointUp) == ESkillFlag.DisableByPointUp:
            case ESkillLevelMethod.Book when proto.Type != 0 && skill.MasterType != ESkillMasterType.Master:
                return;
        }

        if (_player.GetPoint(EPoints.Level) < proto.LevelLimit) return;

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
            EPoints idx; // enum

            switch (proto.Type)
            {
                case ESkillCategoryType.PassiveSkills:
                    idx = EPoints.SubSkill;
                    break;
                case ESkillCategoryType.WarriorSkills: // warrior
                case ESkillCategoryType.NinjaSkills: // ninja
                case ESkillCategoryType.SuraSkills: // sura
                case ESkillCategoryType.ShamanSkills: // shaman
                    idx = EPoints.Skill;
                    break;
                case ESkillCategoryType.HorseSkills:
                    idx = EPoints.HorseSkill;
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
                    if (GetSkillLevel(proto.Id) >= 40)
                    {
                        SetLevel(proto.Id, 40);
                    }

                    break;
            }
        }

        _logger.LogInformation("Skill up: {SkillId} ({Name}) [{Master}] -> {Level}", proto.Id, proto.Name,
            skill.MasterType, GetSkillLevel(proto.Id));

        _player.SendPoints();
        SendSkillLevelsPacket();
    }

    private bool IsLearnableSkill(ESkillIndexes skillId)
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
            return skillId != ESkillIndexes.HorseWildAttackRange ||
                   _player.Player.PlayerClass == (int)EPlayerClass.Ninja;
        }

        if (_player.Player.SkillGroup == 0) return false;

        return (int)proto.Type - 1 == _player.Player.PlayerClass;
    }

    private int GetSkillLevel(ESkillIndexes skillId)
    {
        if (skillId >= ESkillIndexes.SkillMax) return 0;

        return Math.Min(SkillMaxLevel, _skills.TryGetValue(skillId, out var skill) ? skill.Level : 0);
    }

    public bool CanUse(ESkillIndexes skillId)
    {
        if (skillId == 0) return false;

        var skillGroup = _player.Player.SkillGroup;

        if (skillGroup > 0) // if skill group was chosen
        {
            for (var i = 0; i < SkillCount; i++)
            {
                if (SkillList[_player.Player.PlayerClass, skillGroup - 1, i] == skillId)
                {
                    return true;
                }
            }
        }

        // todo: horse riding check

        switch (skillId)
        {
            case ESkillIndexes.Leadership:
            case ESkillIndexes.Combo:
            case ESkillIndexes.Mining:
            case ESkillIndexes.LanguageShinsoo:
            case ESkillIndexes.LanguageChunjo:
            case ESkillIndexes.LanguageJinno:
            case ESkillIndexes.Polymorph:
            case ESkillIndexes.HorseRiding:
            case ESkillIndexes.HorseSummon:
            case ESkillIndexes.GuildEye:
            case ESkillIndexes.GuildBlood:
            case ESkillIndexes.GuildBless:
            case ESkillIndexes.GuildSeonghwi:
            case ESkillIndexes.GuildAcceleration:
            case ESkillIndexes.GuildBunno:
            case ESkillIndexes.GuildJumun:
            case ESkillIndexes.GuildTeleport:
            case ESkillIndexes.GuildDoor:
                return true;
        }

        return false;
    }

    public void SendAsync()
    {
        SendSkillLevelsPacket();
    }

    public bool LearnSkillByBook(ESkillIndexes skillId)
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

        if (_player.GetPoint(EPoints.Experience) < _skillsOptions.SkillBookNeededExperience)
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

        _player.AddPoint(EPoints.Experience, -_skillsOptions.SkillBookNeededExperience);

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

    public void SetSkillNextReadTime(ESkillIndexes skillId, int time)
    {
        if (skillId >= ESkillIndexes.SkillMax)
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
            levels.Skills[i] = new PlayerSkill {Level = 0, MasterType = ESkillMasterType.Normal, NextReadTime = 0};
        }

        for (var i = 0; i < _skills.Count; i++)
        {
            var skill = _skills.ElementAt(i);

            levels.Skills[(uint)skill.Key] = new PlayerSkill
            {
                Level = skill.Value.Level,
                MasterType = skill.Value.MasterType,
                NextReadTime = skill.Value.NextReadTime
            };
        }

        _player.Connection.Send(levels);
    }

    private void AssignDefaultActiveSkills()
    {
        for (var i = 0; i < SkillCount; i++)
        {
            var skill = SkillList[_player.Player.PlayerClass, _player.Player.SkillGroup - 1, i];
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
