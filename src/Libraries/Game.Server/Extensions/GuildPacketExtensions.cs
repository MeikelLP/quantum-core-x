using System.Collections.Immutable;
using QuantumCore.API;
using QuantumCore.API.Game.Guild;
using QuantumCore.API.Game.Types.Guild;
using QuantumCore.API.Packets.Guild;

namespace QuantumCore.Game.Extensions;

public static class GuildPacketExtensions
{
    extension(IConnection connection)
    {
        public void SendGuildNews(ImmutableArray<GuildNewsData> news)
        {
            connection.Send(new GuildNewsPacket
            {
                News = news.Select(x => new GuildNews { NewsId = x.Id, PlayerName = x.PlayerName, Message = x.Message })
                    .ToArray()
            });
        }

        public void SendGuildRanks(ImmutableArray<GuildRankData> ranks)
        {
            connection.Send(new GuildRankPacket
            {
                Ranks = ranks
                    .Select(rank => new GuildRankDataPacket
                    {
                        Rank = rank.Position, Name = rank.Name, Permissions = rank.Permissions
                    })
                    .Take(GuildConstants.RANKS_LENGTH)
                    .ToArray()
            });
        }

        public void SendGuildRankPermissions(byte position,
            GuildRankPermissions permissions)
        {
            connection.Send(new GuildRankPermissionPacket { Position = position, Permissions = permissions });
        }

        public void SendGuildInfo(GuildData guild)
        {
            ArgumentNullException.ThrowIfNull(guild);
            connection.Send(new GuildInfo
            {
                Level = guild.Level,
                Name = guild.Name,
                Gold = guild.Gold,
                GuildId = guild.Id,
                Exp = guild.Experience / 100, // client displays exp * 100
                HasLand = false,
                LeaderId = guild.OwnerId,
                MemberCount = (ushort)guild.Members.Length,
                MaxMemberCount = guild.MaxMemberCount
            });
        }

        public void SendGuildMembers(ImmutableArray<GuildMemberData> members,
            uint[] onlineMemberIds)
        {
            ArgumentNullException.ThrowIfNull(onlineMemberIds);
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
                connection.Send(new GuildMemberOnlinePacket { PlayerId = onlinePlayer });
            }
        }
    }
}