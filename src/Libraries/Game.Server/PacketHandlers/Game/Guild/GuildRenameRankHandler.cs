using QuantumCore.API;
using QuantumCore.API.Game.Guild;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets.Guild;

namespace QuantumCore.Game.PacketHandlers.Game.Guild;

public class GuildRenameRankHandler : IGamePacketHandler<GuildRenameRank>
{
    private readonly IGuildManager _guildManager;

    public GuildRenameRankHandler(IGuildManager guildManager)
    {
        _guildManager = guildManager;
    }

    public async Task ExecuteAsync(GamePacketContext<GuildRenameRank> ctx, CancellationToken token = default)
    {
        if (ctx.Connection.Player!.Player.GuildId is null)
        {
            ctx.Connection.Player.SendChatInfo("You are not part of any guild.");
            return;
        }

        if (!await _guildManager.IsLeaderAsync(ctx.Connection.Player.Player.Id, token))
        {
            ctx.Connection.Player.SendChatInfo("You don't have permission to rename a rank.");
            return;
        }

        if (ctx.Packet.Position == GuildConstants.LEADER_RANK_POSITION)
        {
            ctx.Connection.Player.SendChatInfo("You cannot rename the guild leader rank.");
            return;
        }

        var guildId = ctx.Connection.Player.Player.GuildId.Value;
        await _guildManager.RenameRankAsync(guildId, ctx.Packet.Position, ctx.Packet.Name, token);
        var ranks = await _guildManager.GetRanksAsync(guildId, token);
        ctx.Connection.SendGuildRanks(ranks);
    }
}