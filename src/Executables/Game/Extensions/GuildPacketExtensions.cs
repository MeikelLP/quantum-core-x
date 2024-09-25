using Microsoft.EntityFrameworkCore;
using QuantumCore.API;
using QuantumCore.API.Game.Guild;
using QuantumCore.Game.Packets.Guild;
using QuantumCore.Game.Persistence;

namespace QuantumCore.Game.Extensions;

public static class GuildPacketExtensions
{
    public static async Task SendGuildNewsAsync(this IConnection connection, GameDbContext db, uint guildId,
        CancellationToken token = default)
    {
        var news = await db.GuildNews
            .OrderByDescending(x => x.CreatedAt)
            .Where(x => x.GuildId == guildId)
            .Take(GuildConstants.MAX_NEWS_LOAD)
            .Select(x => new GuildNews
            {
                NewsId = x.Id,
                Message = x.Message,
                PlayerName = x.Player.Name
            })
            .ToArrayAsync(token);
        connection.Send(new GuildNewsPacket
        {
            News = news
        });
    }
}
