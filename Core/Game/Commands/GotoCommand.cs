using System;
using System.Collections.Generic;
using System.Text;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World;
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

        [CommandMethod("Teleports you to a map by their name")]
        public static async void GoToMap(IPlayerEntity player, string mapName)
        {
            var world = World.World.Instance;
            var maps = world.FindMapsByName(mapName);
            if (maps.Count > 1)
            {
                player.SendChatInfo("Map name is ambiguous:");
                foreach (var map in maps)
                {
                    player.SendChatInfo($"- {map.Name}");   
                }

                return;
            }

            if (maps.Count == 0)
            {
                player.SendChatInfo("Unknown map");
                return;
            }
            
            // todo read goto position from map instead of using center

            var targetMap = maps[0];
            player.Move((int)(targetMap.PositionX + targetMap.Width * Map.MapUnit / 2), (int)(targetMap.PositionY + targetMap.Height * Map.MapUnit / 2));
        }
    }
}
