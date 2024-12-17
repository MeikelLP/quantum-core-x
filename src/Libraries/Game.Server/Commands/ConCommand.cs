using QuantumCore.API.Game;

namespace QuantumCore.Game.Commands;

[Command("con+", "Increment health stat by 1")]
public class ConCommand : ICommandHandler
{
    public Task ExecuteAsync(CommandContext context)
    {
        context.Player.Player.Ht++;
        context.Player.SendPoints();
        return Task.CompletedTask;
    }
}
