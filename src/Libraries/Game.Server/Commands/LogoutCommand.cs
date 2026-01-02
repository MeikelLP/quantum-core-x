using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Caching;
using QuantumCore.Game.Systems.Events;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands;

[Command("logout", "Logout from the game")]
[CommandNoPermission]
public class LogoutCommand : ICommandHandler
{
    private readonly IWorld _world;

    private readonly ICacheManager _cacheManager;

    public LogoutCommand(IWorld world, ICacheManager cacheManager)
    {
        _world = world;
        _cacheManager = cacheManager;
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

        player.SendChatInfo("Logging out. Please wait.");

        events.Schedule(events.SafeLogoutCountdown, new SafeLogoutCountdownEvent.Args(
            "{0} seconds until logout.",
            async () =>
            {
                await player.CalculatePlayedTimeAsync();
                await _world.DespawnPlayerAsync(player);
                await _cacheManager.Del("account:token:" + player.Player.AccountId);
                player.Disconnect();
            }));
        return Task.CompletedTask;
    }
}
