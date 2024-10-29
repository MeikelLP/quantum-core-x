using Microsoft.Extensions.Options;
using QuantumCore.API.Game;
using QuantumCore.Game.Extensions;

namespace QuantumCore.Game.Commands;

[Command("in_game_mall", "Opens the in-game item shop webpage")]
[CommandNoPermission]
public class InGameShopCommand : ICommandHandler
{
    private readonly GameOptions _options;

    public InGameShopCommand(IOptions<GameOptions> options)
    {
        _options = options.Value;
    }

    public Task ExecuteAsync(CommandContext context)
    {
        context.Player.SendChatCommand($"mall {_options.InGameShop}");
        return Task.CompletedTask;
    }
}