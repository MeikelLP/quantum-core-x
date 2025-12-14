using QuantumCore.API;
using QuantumCore.API.Game.Guild;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets.Guild;

namespace QuantumCore.Game.PacketHandlers.Game.Guild;

public class GuildRankChangeHandler : IGamePacketHandler<GuildRankChangePacket>
{
    private readonly IGuildManager _guildManager;

    public GuildRankChangeHandler(IGuildManager guildManager)
    {
        _guildManager = guildManager;
    }

    public async Task ExecuteAsync(GamePacketContext<GuildRankChangePacket> ctx, CancellationToken token = default)
    {
        if (ctx.Connection.Player!.Player.GuildId is null)
        {
            ctx.Connection.Player.SendChatInfo("You are not in any guild.");
            return;
        }

        if (!await _guildManager.IsLeaderAsync(ctx.Connection.Player!.Player.Id, token))
        {
            ctx.Connection.Player.SendChatInfo("You don't have permission to change a rank.");
            return;
        }
        else if (ctx.Packet.Position == GuildConstants.LeaderRankPosition)
        {
            ctx.Connection.Player.SendChatInfo("You cannot change the permissions of the guild leader.");
            return;
        }

        var guildId = ctx.Connection.Player!.Player.GuildId.Value;
        await _guildManager.ChangePermissionAsync(guildId, ctx.Packet.Position, ctx.Packet.Permission, token);
        ctx.Connection.SendGuildRankPermissions(ctx.Packet.Position, ctx.Packet.Permission);
        // TODO send to all guild members
    }
}