using Microsoft.EntityFrameworkCore;
using QuantumCore.API.Game.Guild;
using QuantumCore.Game.Persistence;
using QuantumCore.Game.Persistence.Entities.Guilds;
using QuantumCore.Game.Persistence.Extensions;
using GuildMember = QuantumCore.Game.Persistence.Entities.Guilds.GuildMember;

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

    public async Task<GuildData?> GetGuildForPlayerAsync(uint playerId)
    {
        return await _db.Guilds
            .Where(x => x.Members.Any(m => m.PlayerId == playerId))
            .SelectData()
            .FirstOrDefaultAsync();
    }

    public async Task<GuildData> CreateGuildAsync(string name, uint leaderId)
    {
        var leaderRank = new GuildRank
        {
            Rank = 1,
            Name = "Leader",
            Permissions = GuildRankPermission.All
        };
        var guild = new Guild
        {
            Name = name,
            OwnerId = leaderId,
            Level = 1,
            Members = new List<GuildMember>
            {
                new GuildMember
                {
                    PlayerId = leaderId,
                    IsLeader = true,
                    Rank = leaderRank
                }
            },
            Ranks = Enumerable.Range(2, GuildConstants.RANKS_LENGTH - 1)
                .Select(rank => new GuildRank
                {
                    Rank = (byte) rank,
                    Name = "Member"
                })
                .Prepend(leaderRank)
                .ToList()
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
            OwnerId = leaderId
        };
    }
}