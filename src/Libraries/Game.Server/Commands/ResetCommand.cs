using QuantumCore.API.Game;

namespace QuantumCore.Game.Commands;

[Command("r", "Resets the players hp + sp to their max")]
[Command("reset", "Resets the players hp + sp to their max")]
public class ResetCommand : ICommandHandler
{
    public Task ExecuteAsync(CommandContext context)
    {
        context.Player.Health = context.Player.Player.MaxHp;
        context.Player.Mana = context.Player.Player.MaxSp;
        context.Player.SendPoints();

        return Task.CompletedTask;
    }
}
