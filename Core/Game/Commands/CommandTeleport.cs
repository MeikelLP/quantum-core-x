using System;
using System.Collections.Generic;
using System.Text;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands
{
    [Command("tp", "Teleport to another player")]
    public static class CommandTeleportTo
    {
        [CommandMethod]
        public static async void TeleportToPlayer(IPlayerEntity player, IPlayerEntity dest)
        {
            player.SendChatInfo($"Teleporting to player {dest.Name}");
            player.Move(dest.PositionX, dest.PositionY);
        }
    }

    [Command("tphere", "Teleports you to a player")]
    public static class CommandTeleportHere
    {
        [CommandMethod]
        public static async void TeleportToPlayer(IPlayerEntity player, IPlayerEntity dest)
        {
            player.SendChatInfo($"Teleporting {player.Name} to your position");
            dest.Move(player.PositionX, player.PositionY);
        }
    }
}
