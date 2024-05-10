using QuantumCore.API.Game;

namespace QuantumCore.Game.Commands;

[Command("mall_password", "Opens the in-game item-shop warehouse")]
[CommandNoPermission]
public class MallPasswordCommand : ICommandHandler
{
    public Task ExecuteAsync(CommandContext context)
    {
        context.Player.SendChatCommand("ShowMeMallPassword");
        return Task.CompletedTask;
    }
}
