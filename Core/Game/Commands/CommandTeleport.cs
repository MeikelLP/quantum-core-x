using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands
{
    [Command("tp", "Teleport to another player")]
    public static class CommandTeleportTo
    {
        [CommandMethod]
        public static Task TeleportToPlayer(IPlayerEntity player, IPlayerEntity dest)
        {
            player.SendChatInfo($"Teleporting to player {dest.Name}");
            player.Move(dest.PositionX, dest.PositionY);

            return Task.CompletedTask;
        }
    }

    [Command("tphere", "Teleports you to a player")]
    public static class CommandTeleportHere
    {
        [CommandMethod]
        public static Task TeleportToPlayer(IPlayerEntity player, IPlayerEntity dest)
        {
            player.SendChatInfo($"Teleporting {player.Name} to your position");
            dest.Move(player.PositionX, player.PositionY);

            return Task.CompletedTask;
        }
    }
}
