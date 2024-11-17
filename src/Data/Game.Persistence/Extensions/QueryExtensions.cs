using System.Collections.Immutable;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Guild;
using QuantumCore.API.Game.Skills;
using QuantumCore.Game.Persistence.Entities;
using QuantumCore.Game.Persistence.Entities.Guilds;

namespace QuantumCore.Game.Persistence.Extensions;

public static class QueryExtensions
{
    public static IQueryable<PlayerData> SelectPlayerData(this IQueryable<Player> query)
    {
        return query.Select(x => new PlayerData
        {
            Id = x.Id,
            AccountId = x.AccountId,
            Name = x.Name,
            PlayerClass = x.PlayerClass,
            SkillGroup = x.SkillGroup,
            PlayTime = x.PlayTime,
            Level = x.Level,
            Experience = x.Experience,
            Gold = x.Gold,
            St = x.St,
            Ht = x.Ht,
            Dx = x.Dx,
            Iq = x.Iq,
            PositionX = x.PositionX,
            PositionY = x.PositionY,
            Health = x.Health,
            Mana = x.Mana,
            Stamina = x.Stamina,
            BodyPart = x.BodyPart,
            HairPart = x.HairPart,
            GivenStatusPoints = x.GivenStatusPoints,
            AvailableStatusPoints = x.AvailableStatusPoints,
            AvailableSkillPoints = x.AvailableSkillPoints,
            Empire = x.Empire,
            GuildId = x.GuildId
        });
    }

    public static IQueryable<Skill> SelectPlayerSkill(this IQueryable<PlayerSkill> query)
    {
        return query.Select(x => new Skill
        {
            PlayerId = x.PlayerId,
            SkillId = (ESkillIndexes) x.SkillId,
            MasterType = x.MasterType,
            Level = x.Level,
            NextReadTime = x.NextReadTime,
            ReadsRequired = x.ReadsRequired
        });
    }

    public static IQueryable<GuildData> SelectData(this IQueryable<Guild> query)
    {
        return query.Select(x => new GuildData
        {
            Id = x.Id,
            Name = x.Name,
            Level = x.Level,
            Experience = x.Experience,
            Gold = x.Gold,
            OwnerId = x.OwnerId,
            MaxMemberCount = x.MaxMemberCount,
            Members = x.Members.Select(member => new GuildMemberData
            {
                Id = member.Player.Id,
                Name = member.Player.Name,
                Level = member.Player.Level,
                Class = member.Player.PlayerClass,
                SpentExperience = member.SpentExperience,
                Rank = member.RankPosition,
                IsLeader = member.IsLeader
            }).ToImmutableArray(),
            Ranks = x.Ranks.Select(rank => new GuildRankData
            {
                Position = rank.Position,
                Name = rank.Name,
                Permissions = rank.Permissions
            }).ToImmutableArray()
        });
    }

    public static IQueryable<ItemInstance> SelectInstance(this IQueryable<Item> query)
    {
        return query.Select(x => new ItemInstance
        {
            Id = x.Id,
            PlayerId = x.PlayerId,
            ItemId = x.ItemId,
            Window = x.Window,
            Position = x.Position,
            Count = x.Count
        });
    }
}