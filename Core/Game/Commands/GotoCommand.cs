using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
        public static async Task GoToCoordinate(IPlayerEntity player, int x, int y)
        {
            if (x < 0 || y < 0)
                await player.SendChatInfo("The X and Y position must be positive");
            else
                await player.Move((int) player.Map.PositionX + (x*100), (int)player.Map.PositionY + (y*100));
        }

        [CommandMethod("Teleports you to a map by their name")]
        public static async Task GoToMap(IPlayerEntity player, string mapName)
        {
            var world = World.World.Instance;
            var maps = world.FindMapsByName(mapName);
            if (maps.Count > 1)
            {
                await player.SendChatInfo("Map name is ambiguous:");
                foreach (var map in maps)
                {
                    await player.SendChatInfo($"- {map.Name}");   
                }
            }

            if (maps.Count == 0)
            {
                await player.SendChatInfo("Unknown map");
            }
            
            // todo read goto position from map instead of using center

            var targetMap = maps[0];
            await player.Move((int)(targetMap.PositionX + targetMap.Width * Map.MapUnit / 2), (int)(targetMap.PositionY + targetMap.Height * Map.MapUnit / 2));
        }
    }
}
