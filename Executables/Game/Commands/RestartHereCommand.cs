using QuantumCore.API.Game;

namespace QuantumCore.Game.Commands
{
    [Command("restart_here", "Respawns here")]
    [CommandNoPermission]
    public class RestartHereCommand : ICommandHandler
    {
        public Task ExecuteAsync(CommandContext context)
        {
            context.Player.Respawn(false);
            return Task.CompletedTask;
        }
    }
}
