﻿using System.Collections.Immutable;
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
}