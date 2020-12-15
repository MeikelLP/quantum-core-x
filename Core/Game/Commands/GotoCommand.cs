using System;
using System.Collections.Generic;
using System.Text;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands
{
    [Command("goto", "Warp to a position")]
    public static class GotoCommand
    {
        [CommandMethod("X and Y cordinates to teleport to")]
        public static async void GoToCoordinate(IPlayerEntity player, int x, int y)
        {
            if (x < 0 || y < 0)
                player.SendChatInfo("The X and Y position must be positive");
            else
                player.Move((int) player.Map.PositionX + (x*100), (int)player.Map.PositionY + (y*100));
        }
    }
}
