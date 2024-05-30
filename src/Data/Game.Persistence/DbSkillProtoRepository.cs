using Microsoft.EntityFrameworkCore;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.Game.Persistence.Extensions;

namespace QuantumCore.Game.Persistence;

public class DbSkillProtoRepository : IDbSkillProtoRepository
{
    private readonly GameDbContext _db;

    public DbSkillProtoRepository(GameDbContext db)
    {
        _db = db;
    }
    
    public async Task<SkillData?> GetSkill(uint id)
    {
        return await _db.SkillProtos
            .AsNoTracking()
            .Where(x => x.Id == id)
            .SelectSkillData()
            .FirstOrDefaultAsync();
    }

    public async Task<ICollection<SkillData>> GetSkills()
    {
        return await _db.SkillProtos
            .AsNoTracking()
            .SelectSkillData()
            .ToArrayAsync();
    }
}
