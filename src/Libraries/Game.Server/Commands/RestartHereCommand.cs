using QuantumCore.API.Core.Timekeeping;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Constants;

namespace QuantumCore.Game.Commands;

[Command("restart_here", "Respawns here")]
[CommandNoPermission]
public class RestartHereCommand : ICommandHandler
{
    public Task ExecuteAsync(CommandContext context)
    {
        context.Player.RestartWithCooldown(false);
        return Task.CompletedTask;
    }
}

internal static class PlayerRestartCommandExtensions
{
    internal static void RestartWithCooldown(this IPlayerEntity player, bool inTown)
    {
        if (!player.Dead)
        {
            // cannot restart if alive
            return;
        }

        if (player.Timeline[PlayerTimestampKind.DIED] is not { } deathTimestamp)
        {
            // timestamp should be set if dead
            return;
        }

        var clock = player.Connection.Server.Clock;
        var validRestartTimestamp = clock.Advance(deathTimestamp, inTown
            ? SchedulingConstants.RestartTownMinWait
            : SchedulingConstants.RestartHereMinWait);

        if (clock.Now < validRestartTimestamp)
        {
            var remaining = clock.ElapsedBetween(clock.Now, validRestartTimestamp);
            var remainingSeconds = (int)Math.Ceiling(remaining.TotalSeconds);
            player.SendChatInfo($"Cannot restart, please wait {remainingSeconds} seconds.");

            return;
        }

        player.Respawn(inTown);
    }
}
