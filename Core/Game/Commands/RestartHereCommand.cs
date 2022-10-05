using System.Threading.Tasks;
using QuantumCore.API.Game;

namespace QuantumCore.Game.Commands
{
    [Command("restart_here", "Respawns here")]
    [CommandNoPermission]
    public class RestartHereCommand : ICommandHandler
    {
        public async Task ExecuteAsync(CommandContext context)
        {
            await context.Player.Respawn(false);
        }
    }
}