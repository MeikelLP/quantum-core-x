using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.Guild;
using QuantumCore.API.Game.Types.Guild;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets.Guild;

namespace QuantumCore.Game.PacketHandlers.Game.Guild;

public class GuildNewsAddHandler : IGamePacketHandler<GuildNewsAddPacket>
{
    private readonly ILogger<GuildNewsAddPacket> _logger;
    private readonly IGuildManager _guildManager;

    public GuildNewsAddHandler(ILogger<GuildNewsAddPacket> logger, IGuildManager guildManager)
    {
        _logger = logger;
        _guildManager = guildManager;
    }

    public async Task ExecuteAsync(GamePacketContext<GuildNewsAddPacket> ctx, CancellationToken token = default)
    {
        if (ctx.Packet.Message.Length > GuildConstants.NewsMessageMaxLength)
        {
            _logger.LogWarning("Received a guild message that is too long: {Message}", ctx.Packet.Message);
            ctx.Connection.Close(); // equal behaviour as original implementation
            return;
        }

        var player = ctx.Connection.Player!.Player;
        var guildId = player.GuildId;
        if (guildId is null)
        {
            _logger.LogWarning("Player tried to post a message when he has no guild: {Player}", player.Id);
            ctx.Connection.Close(); // equal behaviour as original implementation
            return;
        }

        if (!await _guildManager.HasPermissionAsync(player.Id, GuildRankPermissions.ModifyNews))
        {
            ctx.Connection.Player.SendChatInfo("You don't have permission to create guild news.");
            return;
        }

        _logger.LogDebug("Received new guild news: {Value}", ctx.Packet.Message);
        await _guildManager.CreateNewsAsync(guildId.Value, ctx.Packet.Message, player.Id, token);
        var news = await _guildManager.GetGuildNewsAsync(guildId.Value, token);
        ctx.Connection.SendGuildNews(news);
    }
}
