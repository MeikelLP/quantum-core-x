using QuantumCore.API;
using QuantumCore.API.Game.Guild;
using QuantumCore.API.Game.Types;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets.Guild;

namespace QuantumCore.Game.PacketHandlers.Game.Guild;

public class GuildInviteResponseHandler : IGamePacketHandler<GuildInviteResponse>
{
    private readonly IGuildManager _guildManager;

    public GuildInviteResponseHandler(IGuildManager guildManager)
    {
        _guildManager = guildManager;
    }

    public async Task ExecuteAsync(GamePacketContext<GuildInviteResponse> ctx, CancellationToken token = default)
    {
        var guildId = ctx.Packet.GuildId;
        var guild = await _guildManager.GetGuildByIdAsync(guildId, token);

        var invitee = ctx.Connection.Player!;
        if (guild is null)
        {
            invitee.SendChatInfo("Guild does not exist.");
            return;
        }

        if (invitee.Player.GuildId == guild.Id)
        {
            invitee.SendChatInfo("You are already member in the guild.");
            return;
        }
        // TODO check if player has been invited

        if (ctx.Packet.WantsToJoin)
        {
            var status = guild.CanJoinGuild(invitee);

            switch (status)
            {
                case EGuildJoinStatusCode.AlreadyInAnyGuild:
                    invitee.SendChatInfo("You are already in a guild.");
                    return;
                case EGuildJoinStatusCode.GuildFull:
                    invitee.SendChatInfo("The target guild is full.");
                    return;
            }

            await _guildManager.AddMemberAsync(guildId, invitee.Player.Id, GuildConstants.DEFAULT_JOIN_RANK, token);
            var guildMembers = invitee.Map!.World.GetGuildMembers(guildId);
            foreach (var player in guildMembers)
            {
                player.Connection.Send(new GuildMemberAddPacket
                {
                    PlayerId = invitee.Player.Id
                });
            }

            await invitee.RefreshGuildAsync();
        }
        // TODO what to do when rejected?
    }
}
