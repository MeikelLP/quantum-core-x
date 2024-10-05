using System.Collections.Immutable;
using EnumsNET;
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

    public async Task<GuildData?> GetGuildByNameAsync(string name, CancellationToken token = default)
    {
        return await _db.Guilds
            .Where(x => x.Name == name)
            .SelectData()
            .FirstOrDefaultAsync(token);
    }

    public async Task<GuildData?> GetGuildByIdAsync(uint id, CancellationToken token)
    {
        return await _db.Guilds
            .Where(x => x.Id == id)
            .SelectData()
            .FirstOrDefaultAsync(token);
    }

    public async Task<GuildData?> GetGuildForPlayerAsync(uint playerId, CancellationToken token = default)
    {
        return await _db.Guilds
            .Where(x => x.Members.Any(m => m.PlayerId == playerId))
            .SelectData()
            .FirstOrDefaultAsync(token);
    }

    public async Task<GuildData> CreateGuildAsync(string name, uint leaderId, CancellationToken token = default)
    {
        var leaderRank = new GuildRank
        {
            Position = 1,
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
                    Position = (byte) rank,
                    Name = "Member"
                })
                .Prepend(leaderRank)
                .ToList(),
            MaxMemberCount = GuildConstants.MEMBERS_MAX_DEFAULT
        };
        _db.Guilds.Add(guild);
        await _db.SaveChangesAsync(token);
        await _db.Players
            .Where(x => x.Id == leaderId)
            .ExecuteUpdateAsync(setter => setter
                .SetProperty(p => p.GuildId, guild.Id), token);

        return new GuildData
        {
            Id = guild.Id,
            Name = name,
            Level = 1,
            MaxMemberCount = 10,
            OwnerId = leaderId
        };
    }

    public async Task<uint> CreateNewsAsync(uint guildId, string message, uint playerId,
        CancellationToken token = default)
    {
        var guildNews = new GuildNews
        {
            Message = message,
            PlayerId = playerId,
            CreatedAt = DateTime.UtcNow,
            GuildId = guildId
        };
        _db.GuildNews.Add(guildNews);
        await _db.SaveChangesAsync(token);
        return guildNews.Id;
    }

    public async Task<bool> HasPermissionAsync(uint playerId, GuildRankPermission perm,
        CancellationToken token = default)
    {
        var perms = await _db.GuildMembers
            .Where(x => x.PlayerId == playerId)
            .Select(x => x.Rank.Permissions)
            .FirstAsync(token);

        return perms.HasAnyFlags(perm);
    }

    public async Task<ImmutableArray<GuildNewsData>> GetGuildNewsAsync(uint guildId, CancellationToken token = default)
    {
        return
        [
            ..await _db.GuildNews
                .OrderByDescending(x => x.CreatedAt)
                .Where(x => x.GuildId == guildId)
                .Take(GuildConstants.MAX_NEWS_LOAD)
                .Select(x => new GuildNewsData(x.Id, x.Player.Name, x.Message))
                .ToArrayAsync(token)
        ];
    }

    public async Task<bool> DeleteNewsAsync(uint guildId, uint newsId, CancellationToken token = default)
    {
        return await _db.GuildNews
            .Where(x => x.GuildId == guildId && x.Id == newsId)
            .ExecuteDeleteAsync(token) == 1;
    }

    public Task<bool> IsLeaderAsync(uint playerId, CancellationToken token = default)
    {
        return _db.GuildMembers
            .Where(x => x.PlayerId == playerId && x.IsLeader)
            .AnyAsync(token);
    }

    public async Task<ImmutableArray<GuildRankData>> GetRanksAsync(uint guildId, CancellationToken token)
    {
        return
        [
            ..await _db.GuildRanks
                .Where(x => x.GuildId == guildId)
                .OrderBy(x => x.Position)
                .Take(GuildConstants.RANKS_LENGTH)
                .Select(x => new GuildRankData
                {
                    Name = x.Name,
                    Permissions = x.Permissions,
                    Position = x.Position
                })
                .ToArrayAsync(token)
        ];
    }

    public async Task RenameRankAsync(uint guildId, byte position, string packetName, CancellationToken token)
    {
        await _db.GuildRanks
            .Where(x => x.GuildId == guildId && x.Position == position)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.Name, packetName), token);
    }

    public async Task RemoveGuildAsync(uint guildId, CancellationToken token = default)
    {
        await _db.Guilds
            .Where(x => x.Id == guildId)
            .ExecuteDeleteAsync(token);
    }

    public async Task ChangePermissionAsync(uint guildId, byte position, GuildRankPermission permissions,
        CancellationToken token = default)
    {
        await _db.GuildRanks
            .Where(x => x.GuildId == guildId && x.Position == position)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.Permissions, permissions), token);
    }

    public async Task AddMemberAsync(uint guildId, uint inviteeId, byte rank, CancellationToken token = default)
    {
        _db.GuildMembers.Add(new GuildMember
        {
            PlayerId = inviteeId,
            GuildId = guildId,
            RankPosition = rank
        });
        await _db.SaveChangesAsync(token);
    }

    public async Task RemoveMemberAsync(uint playerId, CancellationToken token = default)
    {
        await _db.GuildMembers
            .Where(x => x.PlayerId == playerId)
            .ExecuteDeleteAsync(token);
    }

    public async Task SetLeaderAsync(uint playerId, bool toggle, CancellationToken token = default)
    {
        await _db.GuildMembers.Where(x => x.PlayerId == playerId)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.IsLeader, toggle), token);
    }
}
