using QuantumCore.API;
using QuantumCore.API.Game.Guild;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets.Guild;

namespace QuantumCore.Game.PacketHandlers.Game.Guild;

public class GuildNewsDeleteHandler : IGamePacketHandler<GuildNewsDelete>
{
    private readonly IGuildManager _guildManager;

    public GuildNewsDeleteHandler(IGuildManager guildManager)
    {
        _guildManager = guildManager;
    }

    public async Task ExecuteAsync(GamePacketContext<GuildNewsDelete> ctx, CancellationToken token = default)
    {
        var guildId = ctx.Connection.Player!.Player.GuildId;

        if (guildId is null)
        {
            ctx.Connection.Player.SendChatInfo("Not part of any guild");
            return;
        }

        if (await _guildManager.DeleteNewsAsync(guildId.Value, ctx.Packet.Id, token))
        {
            var news = await _guildManager.GetGuildNewsAsync(guildId.Value, token);
            ctx.Connection.SendGuildNews(news);
        }
    }
}
