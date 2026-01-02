using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Systems.Events;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands;

[Command("quit", "Quit the game")]
[CommandNoPermission]
public class QuitCommand : ICommandHandler
{
    private readonly IWorld _world;

    public QuitCommand(IWorld world)
    {
        _world = world;
    }

    public Task ExecuteAsync(CommandContext context)
    {
        if (context.Player is not PlayerEntity { Events: var events } player)
        {
            throw new NotImplementedException();
        }

        // toggle mechanism
        if (events.Cancel(events.SafeLogoutCountdown))
        {
            player.SendChatInfo("Your logout has been cancelled.");
            return Task.CompletedTask;
        }

        player.SendChatInfo("End the game. Please wait.");

        events.Schedule(events.SafeLogoutCountdown, new SafeLogoutCountdownEvent.Args(
            "{0} seconds until quit.",
            async () =>
            {
                player.SendChatCommand("quit");
                await player.CalculatePlayedTimeAsync();
                await _world.DespawnPlayerAsync(player);
                player.Disconnect();
            }));

        return Task.CompletedTask;
    }
}
