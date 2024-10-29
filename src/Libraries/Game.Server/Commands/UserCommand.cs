using QuantumCore.API;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;

namespace QuantumCore.Game.Commands;

[Command("user", "List all currently connected players to this server instance")]
public class UserCommand : ICommandHandler
{
    private readonly IGameServer _server;

    public UserCommand(IGameServer server)
    {
        _server = server;
    }

    public Task ExecuteAsync(CommandContext context)
    {
        var message = string.Join("\n",
            _server.Connections.Select(x => $"Lv{x.Player!.GetPoint(EPoints.Level)} {x.Player!.Name}"));

        context.Player.SendChatInfo(message);
        return Task.CompletedTask;
    }
}