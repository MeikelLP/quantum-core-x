using EnumsNET;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.Guild;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets.Guild;
using QuantumCore.Game.Persistence;
using GuildNews = QuantumCore.Game.Persistence.Entities.Guilds.GuildNews;

namespace QuantumCore.Game.PacketHandlers.Game.Guild;

public class GuildNewsAddHandler : IGamePacketHandler<GuildNewsAddPacket>
{
    private readonly ILogger<GuildNewsAddPacket> _logger;
    private readonly GameDbContext _db;

    public GuildNewsAddHandler(ILogger<GuildNewsAddPacket> logger, GameDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task ExecuteAsync(GamePacketContext<GuildNewsAddPacket> ctx, CancellationToken token = default)
    {
        if (ctx.Packet.Message.Length > GuildConstants.NEWS_MESSAGE_MAX_LENGTH)
        {
            _logger.LogWarning("Received a guild message that is too long: {Message}", ctx.Packet.Message);
            ctx.Connection.Close(); // equal behaviour as original implementation
            return;
        }

        var player = ctx.Connection.Player!.Player;
        if (player.GuildId is null)
        {
            _logger.LogWarning("Player tried to post a message when he has no guild: {Player}", player.Id);
            ctx.Connection.Close(); // equal behaviour as original implementation
            return;
        }

        var perms = await _db.GuildMembers
            .Where(x => x.PlayerId == player.Id)
            .Select(x => x.Rank.Permissions)
            .FirstAsync(token);

        if (!perms.HasAnyFlags(GuildRankPermission.ModifyNews))
        {
            ctx.Connection.Player.SendChatInfo("You don't have permission to create guild news.");
            return;
        }

        _logger.LogDebug("Received new guild news: {Value}", ctx.Packet.Message);
        var guildId = player.GuildId.Value;
        _db.GuildNews.Add(new GuildNews
        {
            Message = ctx.Packet.Message,
            PlayerId = player.Id,
            CreatedAt = DateTime.UtcNow,
            GuildId = guildId
        });
        await _db.SaveChangesAsync(token);

        await ctx.Connection.SendGuildNewsAsync(_db, guildId, token);
    }
}
