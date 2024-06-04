using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.Skills;
using QuantumCore.API.Game.Types;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Persistence;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Skills;

public class PlayerSkills : IPlayerSkills
{
    private readonly ConcurrentDictionary<uint, Skill> _skills = new(); //todo: probably no need for concurrent variant
    private readonly ILogger<PlayerSkills> _logger;
    private readonly PlayerEntity _player;
    private readonly IDbPlayerSkillsRepository _repository;
    private readonly ISkillManager _skillManager;

    private const int SkillMaxNum = 255;
    private const int SkillMaxLevel = 40;
    private const int SkillCount = 6;
    private const int JobMaxNum = 4;
    private const int SkillGroupMaxNum = 2;
    
    private static readonly uint[,,] SkillList = new uint[JobMaxNum, SkillGroupMaxNum, SkillCount]
    {
        { { 1,  2,  3,  4,  5,  6  }, { 16,  17,  18,  19,  20,  21  } }, // Warrior
        { { 31, 32, 33, 34, 35, 36 }, { 46,  47,  48,  49,  50,  51  } }, // Ninja
        { { 61, 62, 63, 64, 65, 66 }, { 76,  77,  78,  79,  80,  81  } }, // Sura
        { { 91, 92, 93, 94, 95, 96 }, { 106, 107, 108, 109, 110, 111 } }  // Shaman
    };

    private static readonly uint[] PassiveSkillIds =
    [
        (uint) ESkillIndexes.Leadership,
        (uint) ESkillIndexes.Combo,
        (uint) ESkillIndexes.Mining,
        (uint) ESkillIndexes.LanguageShinsoo,
        (uint) ESkillIndexes.LanguageChunjo,
        (uint) ESkillIndexes.LanguageJinno,
        (uint) ESkillIndexes.Polymorph,
        (uint) ESkillIndexes.HorseRiding,
        (uint) ESkillIndexes.HorseSummon,
        (uint) ESkillIndexes.HoseWildAttack,
        (uint) ESkillIndexes.HorseCharge,
        (uint) ESkillIndexes.HorseEscape,
        (uint) ESkillIndexes.HorseWildAttackRange,
        (uint) ESkillIndexes.AddHp,
        (uint) ESkillIndexes.PenetrationResistance
    ];

    public PlayerSkills(ILogger<PlayerSkills> logger, PlayerEntity player, IDbPlayerSkillsRepository repository, ISkillManager skillManager)
    {
        _logger = logger;
        _player = player;
        _repository = repository;
        _skillManager = skillManager;
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

    public ISKill? this[uint skillId] => _skills.TryGetValue(skillId, out var skill) ? skill : null;

    public void SetSkillGroup(byte skillGroup)
    {
        if (skillGroup > 2) return;
        if (_player.GetPoint(EPoints.Level) < 5) return;
        
        // todo: prevent changing skill group in certain situations
        
        _player.Player.SkillGroup = skillGroup;
        
        AssignDefaultActiveSkills();
        
        _player.Connection.Send(new QuantumCore.Game.Packets.Skills.ChangeSkillGroup
        {
            SkillGroup = skillGroup
        });
    }
    
    public void ClearSkills()
    {
        var points = _player.GetPoint(EPoints.Level) < 5
            ? 0
            : 4 + (_player.GetPoint(EPoints.Level) - 5) - _player.GetPoint(EPoints.Skill);
        _player.SetPoint(EPoints.Skill, points);
            
        ResetSkills();
    }
    
    public void ClearSubSkills()
    {
        var points = _player.GetPoint(EPoints.Level) < 10
            ? 0
            : (_player.GetPoint(EPoints.Level) - 9) - _player.GetPoint(EPoints.SubSkill);
        
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

    public void Reset(uint skillId)
    {
        if (skillId >= SkillMaxNum)
        {
            return;
        }

        if (!_skills.TryGetValue(skillId, out var skill)) return;

        var level = skill.Level;
        
        skill.Level = 0;
        skill.MasterType = ESkillMasterType.Normal;
        skill.NextReadTime = 0;
        
        if (level > 17)
            level = 17;
        
        _player.AddPoint(EPoints.Skill, level);

        SendSkillLevelsPacket();
    }

    public void SetLevel(uint skillId, byte level)
    {
        if (skillId >= SkillMaxNum)
        {
            return;
        }

        if (!_skills.TryGetValue(skillId, out var skill)) return;

        skill.Level = Math.Min((byte) 40, level);

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

    public void SkillUp(uint skillId, ESkillLevelMethod method = ESkillLevelMethod.Point)
    {
        if (skillId >= SkillMaxNum)
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
        
        if (proto.Id >= SkillMaxNum)
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
            _player.SendChatMessage("You cannot learn this skill.");
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
            case ESkillLevelMethod.Point when proto.Flags.Contains(ESkillFlag.DisableByPointUp):
            case ESkillLevelMethod.Book when proto.Type != 0 && skill.MasterType != ESkillMasterType.Master:
                return;
        }

        if (_player.GetPoint(EPoints.Level) < proto.LevelLimit) return;

        if (proto.PrerequisiteSkillVnum > 0)
        {
            if (skill.MasterType == ESkillMasterType.Normal && GetSkillLevel(proto.Id) < proto.PrerequisiteSkillLevel)
            {
                _player.SendChatMessage("You need to learn the prerequisite skill first.");
                return;
            }
        }

        if (_player.Player.SkillGroup == 0) return;
        
        if (method == ESkillLevelMethod.Point)
        {
            EPoints idx; // enum

            switch (proto.Type)
            {
                case 0:
                    idx = EPoints.SubSkill;
                    break;
                case 1: // warrior
                case 2: // ninja
                case 3: // sura
                case 4: // shaman
                    idx = EPoints.Skill;
                    break;
                case 5:
                    idx = EPoints.HorseSkill;
                    break;
                default:
                    _logger.LogWarning("Invalid skill type: {SkillType}", proto.Type);
                    return;
            }
            
            if ((int) idx == 0) return;
            
            if (_player.GetPoint(idx) < 1) return;
            
            _player.AddPoint(idx, -1);
        }
        
        SetLevel(proto.Id, (byte) (GetSkillLevel(proto.Id) + 1));

        if (proto.Type != 0)
        {
            switch (skill.MasterType)
            {
                case ESkillMasterType.Normal:
                    if (GetSkillLevel(proto.Id) >= 17)
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
        
        _logger.LogInformation("Skill up: {SkillId} ({Name}) [{Master}] -> {Level}", proto.Id, proto.Name, skill.MasterType, GetSkillLevel(proto.Id));
        
        _player.SendPoints();
        SendSkillLevelsPacket();
    }

    private bool IsLearnableSkill(uint skillId)
    {
        var proto = _skillManager.GetSkill(skillId);
        if (proto == null)
        {
            return false;
        }
        
        if (GetSkillLevel(skillId) >= SkillMaxLevel) return false;

        if (proto.Type == 0)
        {
            return GetSkillLevel(skillId) < proto.MaxLevel;
        }
        
        if (proto.Type == 5)
        {
            return skillId != (int) ESkillIndexes.HorseWildAttackRange || _player.Player.PlayerClass == (int) EPlayerClass.Ninja;
        }
        
        if (_player.Player.SkillGroup == 0) return false;
        
        return proto.Type - 1 == _player.Player.PlayerClass;
    }

    private int GetSkillLevel(uint skillId)
    {
        if (skillId >= SkillMaxNum)
        {
            return 0;
        }
        
        return Math.Min(SkillMaxLevel, _skills.TryGetValue(skillId, out var skill) ? skill.Level : 0);
    }

    public bool CanUse(uint skillId)
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
            case (uint) ESkillIndexes.Leadership:
            case (uint) ESkillIndexes.Combo:
            case (uint) ESkillIndexes.Mining:
            case (uint) ESkillIndexes.LanguageShinsoo:
            case (uint) ESkillIndexes.LanguageChunjo:
            case (uint) ESkillIndexes.LanguageJinno:
            case (uint) ESkillIndexes.Polymorph:
            case (uint) ESkillIndexes.HorseRiding:
            case (uint) ESkillIndexes.HorseSummon:
            case (uint) ESkillIndexes.GuildEye:
            case (uint) ESkillIndexes.GuildBlood:
            case (uint) ESkillIndexes.GuildBless:
            case (uint) ESkillIndexes.GuildSeonghwi:
            case (uint) ESkillIndexes.GuildAcceleration:
            case (uint) ESkillIndexes.GuildBunno:
            case (uint) ESkillIndexes.GuildJumun:
            case (uint) ESkillIndexes.GuildTeleport:
            case (uint) ESkillIndexes.GuildDoor:
                return true;
        }

        return false;
    }

    public void SendAsync()
    {
        SendSkillLevelsPacket();
    }

    public bool LearnSkillByBook(uint skillId)
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

        if (_player.GetPoint(EPoints.Experience) < SkillsConstants.SKILLBOOK_NEEDED_EXPERIENCE)
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
            _player.SendChatInfo($"You cannot read this skill book yet. {skill.NextReadTime - currentTime} seconds to wait.");
            return false;
        }
        
        _player.AddPoint(EPoints.Experience, -SkillsConstants.SKILLBOOK_NEEDED_EXPERIENCE);
        
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
                    _ => 0
                };
            }
            else
            {
                skill.ReadsRequired--;
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
    
    public void SetSkillNextReadTime(uint skillId, int time)
    {
        if (skillId >= SkillMaxNum)
        {
            return;
        }

        if (!_skills.TryGetValue(skillId, out var skill)) return;

        skill.NextReadTime = time;
    }

    private void SendSkillLevelsPacket()
    {
        var levels = new QuantumCore.Game.Packets.Skills.SkillLevels();
        for (var i = 0; i < SkillMaxNum; i++)
        {
            levels.Skills[i] = new QuantumCore.Game.Packets.Skills.PlayerSkill
            {
                Level = 0,
                MasterType = ESkillMasterType.Normal,
                NextReadTime = 0
            };
        }

        for (var i = 0; i < _skills.Count; i++)
        {
            var skill = _skills.ElementAt(i);
            
            levels.Skills[skill.Key] = new  QuantumCore.Game.Packets.Skills.PlayerSkill
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
            var skillId = SkillList[_player.Player.PlayerClass, _player.Player.SkillGroup - 1, i];
            if (skillId == 0) continue;
            
            _skills[skillId] = new Skill
            {
                Level = 0,
                MasterType = ESkillMasterType.Normal,
                NextReadTime = 0,
                SkillId = skillId,
                PlayerId = _player.Player.Id,
            };
        }
    }
    
    private void AssignDefaultPassiveSkills()
    {
        foreach (var skillId in PassiveSkillIds)
        {
            _skills[skillId] = new Skill
            {
                Level = 0,
                MasterType = ESkillMasterType.Normal,
                NextReadTime = 0,
                SkillId = skillId,
                PlayerId = _player.Player.Id,
            };
        }
    }
}
