using QuantumCore.Packets.Guild;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Guild;
using QuantumCore.PluginTypes;

namespace QuantumCore.Game.PacketHandlers.Game.Guild;

public class GuildRefreshCommentsHandler : IGamePacketHandler<GuildRefreshComments>
{
    private readonly IGuildManager _guildManager;

    public GuildRefreshCommentsHandler(IGuildManager guildManager)
    {
        _guildManager = guildManager;
    }

    public async Task ExecuteAsync(GamePacketContext<GuildRefreshComments> ctx, CancellationToken token = default)
    {
        var guildId = ctx.Connection.Player!.Player.GuildId;

        if (guildId is null)
        {
            ctx.Connection.Player.SendChatInfo("You are in no guild");
            return;
        }

        var news = await _guildManager.GetGuildNewsAsync(guildId.Value, token);
        ctx.Connection.SendGuildNews(news);
    }
}
