using QuantumCore.API.Game;

namespace QuantumCore.Game.Commands;

[Command("int+", "Increment intelligence stat by 1")]
public class IntCommand : ICommandHandler
{
    public Task ExecuteAsync(CommandContext context)
    {
        context.Player.Player.Iq++;
        context.Player.SendPoints();
        return Task.CompletedTask;
    }
}
