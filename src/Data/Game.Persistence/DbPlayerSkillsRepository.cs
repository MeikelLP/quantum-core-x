using Microsoft.EntityFrameworkCore;
using QuantumCore.API.Game.Skills;
using QuantumCore.Game.Persistence.Entities;
using QuantumCore.Game.Persistence.Extensions;

namespace QuantumCore.Game.Persistence;

public class DbPlayerSkillsRepository : IDbPlayerSkillsRepository
{
    private readonly GameDbContext _db;
    
    public DbPlayerSkillsRepository(GameDbContext db)
    {
        _db = db;
    }
    
    public async Task<Skill?> GetPlayerSkillAsync(uint playerId, uint skillId)
    {
        return await _db.PlayerSkills
            .AsNoTracking()
            .Where(x => x.PlayerId == playerId && x.SkillId == skillId)
            .SelectPlayerSkill()
            .FirstOrDefaultAsync();
    }

    public async Task<ICollection<Skill>> GetPlayerSkillsAsync(uint playerId)
    {
        return await _db.PlayerSkills
            .AsNoTracking()
            .Where(x => x.PlayerId == playerId)
            .SelectPlayerSkill()
            .ToArrayAsync();
    }

    public async Task SavePlayerSkillAsync(Skill skill)
    {
        var existingSkill = await _db.PlayerSkills
            .Where(x => x.PlayerId == skill.PlayerId && x.SkillId == (uint) skill.SkillId)
            .FirstOrDefaultAsync();
        
        if (existingSkill != null)
        {
            existingSkill.Level = (byte)skill.Level;
            existingSkill.MasterType = skill.MasterType;
            existingSkill.NextReadTime = skill.NextReadTime;
            existingSkill.UpdatedAt = DateTime.UtcNow;
            existingSkill.ReadsRequired = skill.ReadsRequired;
            _db.PlayerSkills.Update(existingSkill);
            await _db.SaveChangesAsync();
            return;
        }

        var entity = new PlayerSkill
        {
            Id = Guid.NewGuid(),
            PlayerId = skill.PlayerId,
            SkillId = (uint) skill.SkillId,
            MasterType = skill.MasterType,
            Level = (byte)skill.Level,
            NextReadTime = skill.NextReadTime,
            ReadsRequired = skill.ReadsRequired,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.PlayerSkills.Add(entity);
        await _db.SaveChangesAsync();
    }
}
