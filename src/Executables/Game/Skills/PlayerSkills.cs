using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Skills;
using QuantumCore.API.Game.Types;
using QuantumCore.Game.Persistence;
using QuantumCore.Game.World.Entities;
using QuantumCore.Game.Persistence.Entities;

namespace QuantumCore.Game.Skills;

public class PlayerSkills : IPlayerSkills
{
    private readonly ConcurrentDictionary<uint, PlayerSkill> _skills = new(); //todo: probably no need for concurrent variant
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
            _skills[skill.SkillId] = skill as Persistence.Entities.PlayerSkill;
        }
        
        // SendSkillLevelsPacket();
    }

    public async Task PersistAsync()
    {
        foreach (var (id, skill) in _skills)
        {
            // todo: remove this statement below
            if (id > 6) continue;
            await _repository.SavePlayerSkillAsync(new Persistence.Entities.PlayerSkill
            {
                Level = skill.Level,
                MasterType = skill.MasterType,
                NextReadTime = skill.NextReadTime,
                PlayerId = _player.Player.Id,
                SkillId = id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
    }

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
        
        SetLevel(skillId, (byte) (GetSkillLevel(skillId) + 1));
        
        //todo: persist data
        
        SendSkillLevelsPacket();
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
            
            _skills[skillId] = new PlayerSkill
            {
                Level = 0,
                MasterType = ESkillMasterType.Normal,
                NextReadTime = 0,
                SkillId = skillId,
                PlayerId = _player.Player.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
    
    private void AssignDefaultPassiveSkills()
    {
        foreach (var skillId in PassiveSkillIds)
        {
            _skills[skillId] = new PlayerSkill
            {
                Level = 0,
                MasterType = ESkillMasterType.Normal,
                NextReadTime = 0,
                SkillId = skillId,
                PlayerId = _player.Player.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}
