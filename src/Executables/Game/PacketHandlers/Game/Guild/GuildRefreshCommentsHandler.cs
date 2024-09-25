using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets.Guild;
using QuantumCore.Game.Persistence;

namespace QuantumCore.Game.PacketHandlers.Game.Guild;

public class GuildRefreshCommentsHandler : IGamePacketHandler<GuildRefreshComments>
{
    private readonly GameDbContext _db;

    public GuildRefreshCommentsHandler(GameDbContext db)
    {
        _db = db;
    }

    public async Task ExecuteAsync(GamePacketContext<GuildRefreshComments> ctx, CancellationToken token = default)
    {
        var guildId = ctx.Connection.Player!.Player.GuildId;

        if (guildId is null)
        {
            ctx.Connection.Player.SendChatInfo("You are in no guild");
            return;
        }

        await ctx.Connection.SendGuildNewsAsync(_db, guildId.Value, token);
    }
}
