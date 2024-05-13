using Microsoft.EntityFrameworkCore;
using QuantumCore.API;
using QuantumCore.Game.Persistence;
using QuantumCore.Game.Persistence.Entities;
using QuantumCore.Game.Persistence.Extensions;

namespace QuantumCore.Game;

public class GuildManager : IGuildManager
{
    private readonly GameDbContext _db;

    public GuildManager(GameDbContext db)
    {
        _db = db;
    }

    public async Task<GuildData?> GetGuildByNameAsync(string name)
    {
        return await _db.Guilds
            .Where(x => x.Name == name)
            .SelectData()
            .FirstOrDefaultAsync();
    }

    public async Task<GuildData?> GetGuildForPlayerAsync(Guid playerId)
    {
        return await _db.Guilds
            .Where(x => x.Members.Any(m => m.Id == playerId))
            .SelectData()
            .FirstOrDefaultAsync();
    }

    public async Task<GuildData> CreateGuildAsync(string name, Guid leaderId)
    {
        var guild = new Guild
        {
            Name = name,
            LeaderId = leaderId,
            Level = 1
        };
        _db.Guilds.Add(guild);
        await _db.SaveChangesAsync();
        await _db.Players
            .Where(x => x.Id == leaderId)
            .ExecuteUpdateAsync(setter => setter
                .SetProperty(p => p.GuildId, guild.Id));

        return new GuildData
        {
            Id = guild.Id,
            Name = name,
            Level = 1,
            MaxMemberCount = 10,
            LeaderId = leaderId
        };
    }
}