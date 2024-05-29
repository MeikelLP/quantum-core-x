using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Skills;
using QuantumCore.API.Game.Types;
using QuantumCore.Game.Packets.Skills;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Skills;

public class PlayerSkills : IPlayerSkills
{
    private readonly ConcurrentDictionary<uint, IPlayerSkill> _skills = new();
    private readonly ILogger<PlayerSkills> _logger;
    private readonly PlayerEntity _player;
    
    private const int SkillMaxNum = 255;
    private const int SkillMaxLevel = 40;
    private const int SkillCount = 6;
    private const int JobMaxNum = 4;
    private const int SkillGroupMaxNum = 2;

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

    public PlayerSkills(ILogger<PlayerSkills> logger, PlayerEntity player)
    {
        _logger = logger;
        _player = player;
    }

    public Task LoadAsync()
    {
        return Task.CompletedTask;
    }

    public Task SaveAsync()
    {
        throw new NotImplementedException();
    }

    public IPlayerSkill? this[uint skillId]
    {
        get => _skills.TryGetValue(skillId, out var skill) ? skill : null;
        set => _skills[skillId] = value;
    }

    public void SetSkillGroup(byte skillGroup)
    {
        if (skillGroup > 2) return;
        if (_player.GetPoint(EPoints.Level) < 5) return;
        
        // todo: prevent changing skill group in certain situations
        
        _player.Player.SkillGroup = skillGroup;
        
        _player.Connection.Send(new ChangeSkillGroup
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
        
        foreach (var subSkill in subSkills)
        {
            _skills[subSkill.Key] = subSkill.Value;
        }

        SendSkillLevelsPacket();
    }

    private void ResetSubSkills()
    {
        // Assign default values to passive skills
        foreach (var skillId in PassiveSkillIds)
        {
            _skills[skillId] = new PlayerSkill
            {
                Level = 0,
                MasterType = ESkillMasterType.Normal,
                NextReadTime = 0
            };
        }

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
        
        _player.SetPoint(EPoints.Skill, _player.GetPoint(EPoints.Skill) + level);

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
    }

    public void SkillUp(uint skillId)
    {
        if (skillId >= SkillMaxNum)
        {
            return;
        }

        if (!IsLearnableSkill(skillId))
        {
            _player.SendChatMessage("You cannot learn this skill.");
            return;
        }
        
        
        
    }

    private bool IsLearnableSkill(uint skillId)
    {
        //todo: read skill proto information nad get specified skill information
        
        if (GetSkillLevel(skillId) >= SkillMaxLevel)
        {
            return false;
        }
        
        if (_player.Player.SkillGroup == 0)
        {
            return false;
        }
        
        return true; // todo: temporary
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
            var SkillList = new uint[JobMaxNum, SkillGroupMaxNum, SkillCount]
            {
                { { 1,  2,  3,  4,  5,  6 },  { 16,  17,  18,  19,  20,  21 } }, // Warrior
                { { 31, 32, 33, 34, 35, 36 }, { 46,  47,  48,  49,  50,  51 } }, // Ninja
                { { 61, 62, 63, 64, 65, 66 }, { 76,  77,  78,  79,  80,  81 } }, // Sura
                { { 91, 92, 93, 94, 95, 96 }, { 106, 107, 108, 109, 110, 111 } } // Shaman
            };
            
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

    private void SendSkillLevelsPacket()
    {
        _player.Connection.Send(new SkillLevels
        {
            Skills = _skills.Values.Select(sk => new PlayerSkill
            {
                Level = sk.Level,
                MasterType = sk.MasterType,
                NextReadTime = sk.NextReadTime
            }).ToArray()
        });
    }
}
