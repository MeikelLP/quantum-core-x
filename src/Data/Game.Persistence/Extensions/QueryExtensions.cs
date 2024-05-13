using System.Collections.Immutable;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.Game.Persistence.Entities;

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
            Empire = x.Empire
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
            LeaderId = x.LeaderId,
            MaxMemberCount = x.MaxMemberCount,
            Members = x.Members.Select(member => new GuildMemberData
            {
                Id = member.Id,
                Name = member.Name
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
