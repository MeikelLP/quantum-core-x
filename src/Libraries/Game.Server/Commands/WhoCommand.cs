using QuantumCore.API;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;

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
        var empire1Count = players.Count(x => x.Empire == EEmpire.SHINSOO);
        var empire2Count = players.Count(x => x.Empire == EEmpire.CHUNJO);
        var empire3Count = players.Count(x => x.Empire == EEmpire.JINNO);
        var iLocal = _server.Connections.Length;

        context.Player.SendChatInfo(
            $"Total [{allPlayers}] {empire1Count} / {empire2Count} / {empire3Count} (this server {iLocal})");

        return Task.CompletedTask;
    }
}
