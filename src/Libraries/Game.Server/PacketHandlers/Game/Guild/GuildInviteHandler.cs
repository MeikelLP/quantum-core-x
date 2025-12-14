using QuantumCore.API;
using QuantumCore.API.Game.Guild;
using QuantumCore.API.Game.Types.Guild;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets.Guild;

namespace QuantumCore.Game.PacketHandlers.Game.Guild;

public class GuildInviteHandler : IGamePacketHandler<GuildInviteIncoming>
{
    private readonly IGuildManager _guildManager;

    public GuildInviteHandler(IGuildManager guildManager)
    {
        _guildManager = guildManager;
    }

    public async Task ExecuteAsync(GamePacketContext<GuildInviteIncoming> ctx, CancellationToken token = default)
    {
        var inviter = ctx.Connection.Player;
        var invitee = inviter!.Map!.World.GetPlayerById(ctx.Packet.InvitedPlayerId);

        if (invitee is null)
        {
            inviter.SendChatInfo("Target not found.");
            return;
        }

        if (inviter.Player.GuildId is null)
        {
            inviter.SendChatInfo("You are not in any guild.");
            return;
        }

        // TODO check if player is in active quest where invite cannot be accepted
        // TODO check if player blocks guild invites

        if (!await _guildManager.HasPermissionAsync(inviter.Player.Id, GuildRankPermissions.AddMember,
                token))
        {
            inviter.SendChatInfo("You do not have permission to invite members.");
            return;
        }

        if (inviter.Player.Empire != invitee.Empire)
        {
            inviter.SendChatInfo("You cannot invite people from other empires.");
        }

        var guildId = inviter.Player.GuildId.Value;
        var guild = await _guildManager.GetGuildByIdAsync(guildId, token);
        var status = guild!.CanJoinGuild(invitee);
        switch (status)
        {
            case EGuildJoinStatusCode.GuildFull:
                inviter.SendChatInfo("The guild is already full.");
                return;
            case EGuildJoinStatusCode.AlreadyInAnyGuild:
                inviter.SendChatInfo("The other player already belongs to another guild.");
                return;
        }

        // TODO track invite

        invitee.Connection.Send(new GuildInviteOutgoing
        {
            GuildId = guildId,
            GuildName = guild!.Name
        });
    }
}