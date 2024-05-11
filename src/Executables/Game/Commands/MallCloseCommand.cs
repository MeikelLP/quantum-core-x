using QuantumCore.API.Game;

namespace QuantumCore.Game.Commands;

[Command("mall_close", "Closes the in-game item-shop warehouse")]
[CommandNoPermission]
public class MallCloseCommand : ICommandHandler
{
    public Task ExecuteAsync(CommandContext context)
    {
        context.Player.Mall.Close();
        return Task.CompletedTask;
    }
}
