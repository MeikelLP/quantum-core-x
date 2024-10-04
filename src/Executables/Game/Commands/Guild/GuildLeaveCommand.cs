﻿using QuantumCore.API.Game;
using QuantumCore.API.Game.Guild;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets.Guild;

namespace QuantumCore.Game.Commands.Guild;

[Command("guild_leave", "Leave your guild")]
[CommandNoPermission]
public class GuildLeaveCommand : ICommandHandler
{
    private readonly IGuildManager _guildManager;

    public GuildLeaveCommand(IGuildManager guildManager)
    {
        _guildManager = guildManager;
    }

    public async Task ExecuteAsync(CommandContext context)
    {
        var player = context.Player.Player;
        var playerId = context.Player.Player.Id;
        var guildId = player.GuildId;

        if (guildId is null)
        {
            context.Player.SendChatInfo("You aren't member in any guild.");
            return;
        }

        var guild = await _guildManager.GetGuildByIdAsync(guildId.Value);
        if (guild is null)
        {
            context.Player.SendChatInfo("Guild not found.");
            return;
        }

        if (guild.OwnerId == playerId)
        {
            context.Player.SendChatInfo(
                "You can't leave the guild if you are the owner. You must disband the guild or transfer ownership.");
            return;
        }

        await _guildManager.RemoveMemberAsync(playerId);
        await context.Player.RefreshGuildAsync();
        foreach (var p in context.Player.GetNearbyPlayers())
        {
            if (p.Player.GuildId == guild.Id)
            {
                // send member update to guild colleagues
                p.Connection.Send(new GuildMemberRemovePacket
                {
                    PlayerId = playerId
                });
            }
        }
    }
}