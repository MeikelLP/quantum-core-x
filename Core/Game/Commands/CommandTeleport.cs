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
        public static async Task TeleportToPlayer(IPlayerEntity player, IPlayerEntity dest)
        {
            await player.SendChatInfo($"Teleporting to player {dest.Name}");
            await player.Move(dest.PositionX, dest.PositionY);
        }
    }

    [Command("tphere", "Teleports you to a player")]
    public static class CommandTeleportHere
    {
        [CommandMethod]
        public static async Task TeleportToPlayer(IPlayerEntity player, IPlayerEntity dest)
        {
            await player.SendChatInfo($"Teleporting {player.Name} to your position");
            await dest.Move(player.PositionX, player.PositionY);
        }
    }
}
