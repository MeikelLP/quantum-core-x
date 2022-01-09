using QuantumCore.API.Game;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Commands
{
    [Command("restart_here", "Respawns here")]
    [CommandNoPermission]
    public class RestartHereCommand
    {
        [CommandMethod]
        public static void Restart(IPlayerEntity player)
        {
            player.Respawn(false);
        }
    }
    
    [Command("restart_town", "Respawns in town")]
    [CommandNoPermission]
    public class RestartTownCommand
    {
        [CommandMethod]
        public static void Restart(IPlayerEntity player)
        {
            player.Respawn(true);
        }
    }
}