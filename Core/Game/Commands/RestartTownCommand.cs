using System.Threading.Tasks;
using QuantumCore.API.Game;

namespace QuantumCore.Game.Commands;

[Command("restart_town", "Respawns in town")]
[CommandNoPermission]
public class RestartTownCommand : ICommandHandler
{
    public async Task ExecuteAsync(CommandContext context)
    {
        await context.Player.Respawn(true);
    }
}