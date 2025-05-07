using QuantumCore.API.Game;

namespace QuantumCore.Game.Commands;

[Command("dex+", "Increment dexterity stat by 1")]
public class DexCommand : ICommandHandler
{
    public Task ExecuteAsync(CommandContext context)
    {
        context.Player.Player.Dx++;
        context.Player.SendPoints();
        return Task.CompletedTask;
    }
}
