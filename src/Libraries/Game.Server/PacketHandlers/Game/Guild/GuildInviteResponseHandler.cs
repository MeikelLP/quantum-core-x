using QuantumCore.API;
using QuantumCore.API.Game.Guild;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.Types.Guild;
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

            const byte rank = GuildConstants.DEFAULT_JOIN_RANK;
            await _guildManager.AddMemberAsync(guildId, invitee.Player.Id, rank, token);

            invitee.Player.GuildId = guildId;
            await invitee.RefreshGuildAsync();

            var guildMembers = invitee.Map!.World.GetGuildMembers(guildId);
            foreach (var player in guildMembers)
            {
                player.Connection.SendGuildMembers([
                    new GuildMemberData
                    {
                        Id = invitee.Player.Id,
                        Name = invitee.Player.Name,
                        Rank = rank,
                        SpentExperience = 0,
                        Class = invitee.Player.PlayerClass,
                        Level = invitee.Player.Level,
                        IsLeader = false
                    }
                ], [invitee.Player.Id]);
                player.Connection.Send(new GuildMemberOnlinePacket
                {
                    PlayerId = invitee.Player.Id
                });
            }
        }
        // TODO what to do when rejected?
    }
}