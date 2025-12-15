using System.Collections.Concurrent;
using System.Collections.Immutable;
using EnumsNET;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuantumCore.API;
using QuantumCore.API.Extensions;
using QuantumCore.API.Game.Skills;
using QuantumCore.API.Game.Types.Entities;
using QuantumCore.API.Game.Types.Players;
using QuantumCore.API.Game.Types.Skills;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets.Skills;
using QuantumCore.Game.Persistence;
using QuantumCore.Game.World.Entities;
using static QuantumCore.API.Game.Types.Skills.ESkillLevelUtils;

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

    public const int SKILL_MAX_NUM = byte.MaxValue;
    public const ESkillLevel SKILL_MAX_LEVEL = ESkillLevel.PERFECT_MASTER_P;
    public const int SKILL_COUNT = 6;
    public const int JOB_MAX_NUM = 4;
    public const int SKILL_GROUP_MAX_NUM = 2;
    public const int MINIMUM_LEVEL = 5;
    public const int MINIMUM_LEVEL_SUB_SKILLS = 10;
    public const ESkillLevel MINIMUM_SKILL_LEVEL_UPGRADE = ESkillLevel.NORMAL17;

    #region Static Skill Data

    private static readonly ESkill[,,] SkillList = new ESkill[JOB_MAX_NUM, SKILL_GROUP_MAX_NUM, SKILL_COUNT]
    {
        // Warrior
        {
            {
                ESkill.THREE_WAY_CUT, ESkill.SWORD_SPIN, ESkill.BERSERKER_FURY,
                ESkill.AURA_OF_THE_SWORD, ESkill.DASH, ESkill.LIFE
            },
            {
                ESkill.SHOCKWAVE, ESkill.BASH, ESkill.STUMP, ESkill.STRONG_BODY,
                ESkill.SWORD_STRIKE, ESkill.SWORD_ORB
            }
        },
        // Ninja
        {
            {
                ESkill.AMBUSH, ESkill.FAST_ATTACK, ESkill.ROLLING_DAGGER, ESkill.STEALTH,
                ESkill.POISONOUS_CLOUD, ESkill.INSIDIOUS_POISON
            },
            {
                ESkill.REPETITIVE_SHOT, ESkill.ARROW_SHOWER, ESkill.FIRE_ARROW,
                ESkill.FEATHER_WALK,
                ESkill.POISON_ARROW, ESkill.SPARK
            }
        },
        // Sura
        {
            {
                ESkill.FINGER_STRIKE, ESkill.DRAGON_SWIRL, ESkill.ENCHANTED_BLADE, ESkill.FEAR,
                ESkill.ENCHANTED_ARMOR, ESkill.DISPEL
            },
            {
                ESkill.DARK_STRIKE, ESkill.FLAME_STRIKE, ESkill.FLAME_SPIRIT,
                ESkill.DARK_PROTECTION, ESkill.SPIRIT_STRIKE, ESkill.DARK_ORB
            }
        },
        // Shaman
        {
            {
                ESkill.FLYING_TALISMAN, ESkill.SHOOTING_DRAGON, ESkill.DRAGON_ROAR,
                ESkill.BLESSING, ESkill.REFLECT, ESkill.DRAGON_AID
            },
            {
                ESkill.LIGHTNING_THROW, ESkill.SUMMON_LIGHTNING, ESkill.LIGHTNING_CLAW,
                ESkill.CURE, ESkill.SWIFTNESS, ESkill.ATTACK_UP
            }
        }
    };

    private static readonly ImmutableArray<ESkill> PassiveSkillIds =
    [
        ESkill.LEADERSHIP,
        ESkill.COMBO,
        ESkill.MINING,
        ESkill.LANGUAGE_SHINSOO,
        ESkill.LANGUAGE_CHUNJO,
        ESkill.LANGUAGE_JINNO,
        ESkill.POLYMORPH,
        ESkill.HORSE_RIDING,
        ESkill.HORSE_SUMMON,
        ESkill.HORSE_WILD_ATTACK,
        ESkill.HORSE_CHARGE,
        ESkill.HORSE_ESCAPE,
        ESkill.HORSE_WILD_ATTACK_RANGE,
        ESkill.ADD_HP,
        ESkill.PENETRATION_RESISTANCE
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
        if (_player.Player.SkillGroup.IsDefined())
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

    public void SetSkillGroup(ESkillGroup skillGroup)
    {
        if (!skillGroup.IsDefined() && skillGroup != 0) return;
        if (_player.GetPoint(EPoint.LEVEL) < MINIMUM_LEVEL) return;

        // todo: prevent changing skill group in certain situations

        _player.Player.SkillGroup = skillGroup;

        AssignDefaultActiveSkills();

        _player.Connection.Send(new ChangeSkillGroup {SkillGroup = skillGroup});
    }

    public void ClearSkills()
    {
        var points = _player.GetPoint(EPoint.LEVEL) < MINIMUM_LEVEL
            ? 0
            : (MINIMUM_LEVEL - 1) + (_player.GetPoint(EPoint.LEVEL) - MINIMUM_LEVEL) - _player.GetPoint(EPoint.SKILL);
        _player.SetPoint(EPoint.SKILL, points);

        ResetSkills();
    }

    public void ClearSubSkills()
    {
        var points = _player.GetPoint(EPoint.LEVEL) < MINIMUM_LEVEL_SUB_SKILLS
            ? 0
            : (_player.GetPoint(EPoint.LEVEL) - (MINIMUM_LEVEL_SUB_SKILLS - 1)) - _player.GetPoint(EPoint.SUB_SKILL);

        _player.SetPoint(EPoint.SUB_SKILL, points);

        ResetSubSkills();
    }

    private void ResetSkills()
    {
        // store subskills in a temporary variable, clear the dictionary and then restore the subskills
        var subSkills = _skills
            .Where(sk => PassiveSkillIds.Contains(sk.Key))
            .ToDictionary(sk => sk.Key, sk => sk.Value);

        _skills.Clear();

        if (_player.Player.SkillGroup.IsDefined())
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
        if (!skillId.IsDefined())
        {
            return;
        }

        if (!_skills.TryGetValue(skillId, out var skill)) return;

        var effectiveLevelForRestore = skill.Level < MINIMUM_SKILL_LEVEL_UPGRADE
            ? skill.Level
            : MINIMUM_SKILL_LEVEL_UPGRADE;

        _player.AddPoint(EPoint.SKILL, (byte)effectiveLevelForRestore);
        
        skill.Level = ESkillLevel.UNLEARNED;
        skill.MasterType = ESkillMasterType.NORMAL;
        skill.NextReadTime = 0;

        SendSkillLevelsPacket();
    }

    public void SetLevel(ESkill skillId, ESkillLevel level)
    {
        if (!skillId.IsDefined())
        {
            return;
        }

        if (!_skills.TryGetValue(skillId, out var skill)) return;

        skill.Level = Min(SKILL_MAX_LEVEL, level);

        skill.MasterType = level switch
        {
            >= SKILL_MAX_LEVEL => ESkillMasterType.PERFECT_MASTER,
            >= ESkillLevel.GRAND_MASTER_G1 => ESkillMasterType.GRAND_MASTER,
            >= ESkillLevel.MASTER_M1 => ESkillMasterType.MASTER,
            _ => ESkillMasterType.NORMAL
        };

        // Reset reads required when new master type is learned
        switch (skill)
        {
            case {Level: ESkillLevel.MASTER_M1, ReadsRequired: 0, MasterType: ESkillMasterType.MASTER}:
            case {Level: ESkillLevel.GRAND_MASTER_G1, ReadsRequired: 0, MasterType: ESkillMasterType.GRAND_MASTER}:
                skill.ReadsRequired = 1;
                break;
        }
    }

    public void SkillUp(ESkill skillId, ESkillLevelMethod method = ESkillLevelMethod.POINT)
    {
        if (!skillId.IsDefined())
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

        if (!proto.Id.IsDefined())
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
                case ESkillMasterType.GRAND_MASTER:
                    if (method != ESkillLevelMethod.QUEST) return;
                    break;
                case ESkillMasterType.PERFECT_MASTER:
                    return;
            }
        }

        switch (method)
        {
            case ESkillLevelMethod.POINT when skill.MasterType != ESkillMasterType.NORMAL:
            case ESkillLevelMethod.POINT when (proto.Flags & ESkillFlags.DISABLE_BY_POINT_UP) == ESkillFlags.DISABLE_BY_POINT_UP:
            case ESkillLevelMethod.BOOK when proto.Type != 0 && skill.MasterType != ESkillMasterType.MASTER:
                return;
        }

        if (_player.GetPoint(EPoint.LEVEL) < proto.LevelLimit) return;

        if (proto.PrerequisiteSkillVnum > 0)
        {
            var prerequisiteSkillLevel = (ESkillLevel)proto.PrerequisiteSkillLevel;
            if (skill.MasterType == ESkillMasterType.NORMAL && GetSkillLevel(proto.Id) < prerequisiteSkillLevel)
            {
                _player.SendChatInfo("You need to learn the prerequisite skill first.");
                return;
            }
        }

        if (!_player.Player.SkillGroup.IsDefined()) return;

        if (method == ESkillLevelMethod.POINT)
        {
            EPoint idx; // enum

            switch (proto.Type)
            {
                case ESkillCategoryType.PASSIVE_SKILLS:
                    idx = EPoint.SUB_SKILL;
                    break;
                case ESkillCategoryType.WARRIOR_SKILLS: // warrior
                case ESkillCategoryType.NINJA_SKILLS: // ninja
                case ESkillCategoryType.SURA_SKILLS: // sura
                case ESkillCategoryType.SHAMAN_SKILLS: // shaman
                    idx = EPoint.SKILL;
                    break;
                case ESkillCategoryType.HORSE_SKILLS:
                    idx = EPoint.HORSE_SKILL;
                    break;
                default:
                    _logger.LogWarning("Invalid skill type: {SkillType}", proto.Type);
                    return;
            }

            if ((int)idx == 0) return;

            if (_player.GetPoint(idx) < 1) return;

            _player.AddPoint(idx, -1);
        }

        SetLevel(proto.Id, GetSkillLevel(proto.Id) + 1);

        if (proto.Type != ESkillCategoryType.PASSIVE_SKILLS)
        {
            switch (skill.MasterType)
            {
                case ESkillMasterType.NORMAL:
                    if (GetSkillLevel(proto.Id) >= MINIMUM_SKILL_LEVEL_UPGRADE)
                    {
                        //todo: implement reset scroll quest flag
                        var effectiveLevel = Min(ESkillLevel.MASTER_M1, GetSkillLevel(proto.Id));
                        var levelsUnder = ESkillLevel.MASTER_M2 - effectiveLevel;
                        var random = CoreRandom.GenerateInt32(1, levelsUnder + 1);
                        if (random == 1)
                        {
                            SetLevel(proto.Id, ESkillLevel.MASTER_M1);
                        }
                    }

                    break;
                case ESkillMasterType.MASTER:
                    if (GetSkillLevel(proto.Id) >= ESkillLevel.GRAND_MASTER_G1)
                    {
                        var effectiveLevel = Min(ESkillLevel.GRAND_MASTER_G1, GetSkillLevel(proto.Id));
                        var levelsUnder = ESkillLevel.GRAND_MASTER_G2 - effectiveLevel;
                        var random = CoreRandom.GenerateInt32(1, levelsUnder + 1);
                        if (random == 1)
                        {
                            SetLevel(proto.Id, ESkillLevel.GRAND_MASTER_G1);
                        }
                    }

                    break;
                case ESkillMasterType.GRAND_MASTER:
                    if (GetSkillLevel(proto.Id) >= ESkillLevel.PERFECT_MASTER_P)
                    {
                        SetLevel(proto.Id, ESkillLevel.PERFECT_MASTER_P);
                    }

                    break;
            }
        }

        _logger.LogInformation("Skill up: {SkillId} ({Name}) [{Master}] -> {Level} ({LevelName})", proto.Id, proto.Name,
            skill.MasterType, (byte)GetSkillLevel(proto.Id), GetSkillLevel(proto.Id).GetName());

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

        if (GetSkillLevel(skillId) >= SKILL_MAX_LEVEL) return false;

        if (proto.Type == ESkillCategoryType.PASSIVE_SKILLS)
        {
            return GetSkillLevel(skillId) < (ESkillLevel)proto.MaxLevel;
        }

        if (proto.Type == ESkillCategoryType.HORSE_SKILLS)
        {
            return skillId != ESkill.HORSE_WILD_ATTACK_RANGE ||
                   _player.Player.PlayerClass.GetClass()== EPlayerClass.NINJA;
        }

        if (!_player.Player.SkillGroup.IsDefined()) return false;

        return (int)proto.Type - 1 == (byte)_player.Player.PlayerClass;
    }

    private ESkillLevel GetSkillLevel(ESkill skillId)
    {
        if (!skillId.IsDefined() || !_skills.TryGetValue(skillId, out var skill))
        {
            return ESkillLevel.UNLEARNED;
        }

        return Min(SKILL_MAX_LEVEL, skill.Level);
    }

    public bool CanUse(ESkill skillId)
    {
        if (skillId == 0) return false;

        var skillGroup = _player.Player.SkillGroup;

        if (skillGroup.IsDefined()) // if skill group was chosen
        {
            for (var i = 0; i < SKILL_COUNT; i++)
            {
                if (SkillList[(int)_player.Player.PlayerClass, (byte)skillGroup - 1, i] == skillId)
                {
                    return true;
                }
            }
        }

        // todo: horse riding check

        switch (skillId)
        {
            case ESkill.LEADERSHIP:
            case ESkill.COMBO:
            case ESkill.MINING:
            case ESkill.LANGUAGE_SHINSOO:
            case ESkill.LANGUAGE_CHUNJO:
            case ESkill.LANGUAGE_JINNO:
            case ESkill.POLYMORPH:
            case ESkill.HORSE_RIDING:
            case ESkill.HORSE_SUMMON:
            case ESkill.GUILD_EYE:
            case ESkill.GUILD_BLOOD:
            case ESkill.GUILD_BLESS:
            case ESkill.GUILD_SEONGHWI:
            case ESkill.GUILD_ACCELERATION:
            case ESkill.GUILD_BUNNO:
            case ESkill.GUILD_JUMUN:
            case ESkill.GUILD_TELEPORT:
            case ESkill.GUILD_DOOR:
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

        if (_player.GetPoint(EPoint.EXPERIENCE) < _skillsOptions.SkillBookNeededExperience)
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
            if (skill.MasterType != ESkillMasterType.MASTER)
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

        _player.AddPoint(EPoint.EXPERIENCE, -_skillsOptions.SkillBookNeededExperience);

        var previousLevel = skill.Level;

        var readSuccess = CoreRandom.GenerateInt32(1, 3) == 1;

        if (readSuccess)
        {
            if (skill.ReadsRequired - 1 == 0)
            {
                SkillUp(skillId, ESkillLevelMethod.BOOK);
                skill.ReadsRequired = skill.Level switch
                {
                    ESkillLevel.MASTER_M2 => 2,
                    ESkillLevel.MASTER_M3 => 3,
                    ESkillLevel.MASTER_M4 => 4,
                    ESkillLevel.MASTER_M5 => 5,
                    ESkillLevel.MASTER_M6 => 6,
                    ESkillLevel.MASTER_M7 => 7,
                    ESkillLevel.MASTER_M8 => 8,
                    ESkillLevel.MASTER_M9 => 9,
                    ESkillLevel.MASTER_M10 => 10,
                    ESkillLevel.GRAND_MASTER_G1 => 1,
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
        if (!skillId.IsDefined())
        {
            return;
        }

        if (!_skills.TryGetValue(skillId, out var skill)) return;

        skill.NextReadTime = time;
    }

    private void SendSkillLevelsPacket()
    {
        var levels = new SkillLevels();
        for (var i = 0; i < SKILL_MAX_NUM; i++)
        {
            levels.Skills[i] = new PlayerSkill {Level = ESkillLevel.UNLEARNED, MasterType = ESkillMasterType.NORMAL, NextReadTime = 0};
        }

        for (var i = 0; i < _skills.Count; i++)
        {
            var skill = _skills.ElementAt(i);

            levels.Skills[(byte)skill.Key] = new PlayerSkill
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
        for (var i = 0; i < SKILL_COUNT; i++)
        {
            var skill = SkillList[(int)_player.Player.PlayerClass.GetClass(), (byte)_player.Player.SkillGroup - 1, i];
            if (skill == 0) continue;

            _skills[skill] = new Skill
            {
                Level = ESkillLevel.UNLEARNED,
                MasterType = ESkillMasterType.NORMAL,
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
                Level = ESkillLevel.UNLEARNED,
                MasterType = ESkillMasterType.NORMAL,
                NextReadTime = 0,
                SkillId = skill,
                PlayerId = _player.Player.Id,
            };
        }
    }
}
