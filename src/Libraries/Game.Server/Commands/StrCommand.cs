using QuantumCore.API.Game;

namespace QuantumCore.Game.Commands;

[Command("str+", "Increment strength stat by 1")]
public class StrCommand : ICommandHandler
{
    public Task ExecuteAsync(CommandContext context)
    {
        context.Player.Player.St++;
        context.Player.SendPoints();
        return Task.CompletedTask;
    }
}
