using QuantumCore.API;
using QuantumCore.API.Game.Guild;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets.Guild;

namespace QuantumCore.Game.PacketHandlers.Game.Guild;

public class GuildMemberMakeLeaderHandler : IGamePacketHandler<GuildMemberMakeLeaderPacket>
{
    private readonly IGuildManager _guildManager;

    public GuildMemberMakeLeaderHandler(IGuildManager guildManager)
    {
        _guildManager = guildManager;
    }

    public async Task ExecuteAsync(GamePacketContext<GuildMemberMakeLeaderPacket> ctx,
        CancellationToken token = default)
    {
        var guildId = ctx.Connection.Player!.Player.GuildId;
        if (guildId is null)
        {
            ctx.Connection.Player.SendChatInfo("You are not in a guild.");
            return;
        }

        var playerId = ctx.Connection.Player.Player.Id;
        if (!await _guildManager.IsLeaderAsync(playerId, token))
        {
            ctx.Connection.Player.SendChatInfo("You don't have permission to set a leader.");
            return;
        }

        var targetPlayerId = ctx.Packet.PlayerId;
        var guild = await _guildManager.GetGuildByIdAsync(guildId.Value, token);
        if (guild!.OwnerId == targetPlayerId)
        {
            ctx.Connection.Player.SendChatInfo("You can't modify the leader state of the guild owner.");
            return;
        }

        var isLeader = ctx.Packet.IsLeader;
        await _guildManager.SetLeaderAsync(targetPlayerId, isLeader, token);
        var members = ctx.Connection.Player.Map!.World.GetGuildMembers(guildId.Value);
        foreach (var member in members)
        {
            member.Connection.Send(new GuildMemberLeaderChangePacket
            {
                PlayerId = targetPlayerId,
                IsLeader = isLeader
            });
        }
    }
}
