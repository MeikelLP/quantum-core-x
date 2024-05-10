using QuantumCore.API.Game;

namespace QuantumCore.Game.Commands;

[Command("click_mall", "Opens the in-game item-shop password prompt")]
[CommandNoPermission]
public class OpenMallCommand : ICommandHandler
{
    public Task ExecuteAsync(CommandContext context)
    {
        context.Player.SendChatCommand("ShowMeMallPassword");
        return Task.CompletedTask;
    }
}
