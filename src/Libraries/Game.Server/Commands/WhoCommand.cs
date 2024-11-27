using QuantumCore.API;
using QuantumCore.API.Game;

namespace QuantumCore.Game.Commands;

[Command("who", "Sends player counts for the current server core, each empire and total across all server cores")]
public class WhoCommand : ICommandHandler
{
    private readonly IGameServer _server;

    public WhoCommand(IGameServer server)
    {
        _server = server;
    }

    public Task ExecuteAsync(CommandContext context)
    {
        var players = context.Player.Map!.World.GetPlayers();
        var allPlayers = players.Count;
        var empire1Count = players.Count(x => x.Empire == 1);
        var empire2Count = players.Count(x => x.Empire == 2);
        var empire3Count = players.Count(x => x.Empire == 3);
        var iLocal = _server.Connections.Length;

        context.Player.SendChatInfo(
            $"Total [{allPlayers}] {empire1Count} / {empire2Count} / {empire3Count} (this server {iLocal})");

        return Task.CompletedTask;
    }
}
