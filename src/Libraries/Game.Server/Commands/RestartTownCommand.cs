using QuantumCore.API.Game;

namespace QuantumCore.Game.Commands;

[Command("restart_town", "Respawns in town")]
[CommandNoPermission]
public class RestartTownCommand : ICommandHandler
{
    public Task ExecuteAsync(CommandContext context)
    {
        context.Player.RestartWithCooldown(true);
        return Task.CompletedTask;
    }
}
