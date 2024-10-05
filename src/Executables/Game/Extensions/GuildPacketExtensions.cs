using System.Collections.Immutable;
using QuantumCore.API;
using QuantumCore.API.Game.Guild;
using QuantumCore.Game.Packets.Guild;

namespace QuantumCore.Game.Extensions;

public static class GuildPacketExtensions
{
    public static void SendGuildNews(this IConnection connection, ImmutableArray<GuildNewsData> news)
    {
        connection.Send(new GuildNewsPacket
        {
            News = news.Select(x => new GuildNews
            {
                NewsId = x.Id,
                PlayerName = x.PlayerName,
                Message = x.Message
            }).ToArray()
        });
    }

    public static void SendGuildRanks(this IConnection connection, ImmutableArray<GuildRankData> ranks)
    {
        connection.Send(new GuildRankPacket
        {
            Ranks = ranks
                .Select(rank => new GuildRankDataPacket
                {
                    Rank = rank.Position,
                    Name = rank.Name,
                    Permissions = rank.Permissions
                })
                .Take(GuildConstants.RANKS_LENGTH)
                .ToArray()
        });
    }

    public static void SendGuildRankPermissions(this IConnection connection, byte position,
        GuildRankPermission permissions)
    {
        connection.Send(new GuildRankPermissionPacket
        {
            Position = position,
            Permissions = permissions
        });
    }

    public static void SendGuildInfo(this IConnection connection, GuildData guild)
    {
        connection.Send(new GuildInfo
        {
            Level = guild.Level,
            Name = guild.Name,
            Gold = guild.Gold,
            GuildId = guild.Id,
            Exp = guild.Experience,
            HasLand = false,
            LeaderId = guild.OwnerId,
            MemberCount = (ushort) guild.Members.Length,
            MaxMemberCount = guild.MaxMemberCount
        });
    }

    public static void SendGuildMembers(this IConnection connection, ImmutableArray<GuildMemberData> members,
        uint[] onlineMemberIds)
    {
        connection.Send(new GuildMemberPacket
        {
            Members = members
                .Select(guildMember => new GuildMember
                {
                    PlayerId = guildMember.Id,
                    Class = guildMember.Class,
                    Level = guildMember.Level,
                    IsGeneral = guildMember.IsLeader,
                    Name = guildMember.Name,
                    Rank = guildMember.Rank,
                    SpentExperience = guildMember.SpentExperience,
                    IsNameSent = true
                })
                .ToArray()
        });
        foreach (var onlinePlayer in onlineMemberIds)
        {
            connection.Send(new GuildMemberOnlinePacket
            {
                PlayerId = onlinePlayer
            });
        }
    }
}
