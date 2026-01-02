using QuantumCore.API.Core.Event;
using QuantumCore.API.Game.World;
using static QuantumCore.Game.Constants.SchedulingConstants;

namespace QuantumCore.Game.Systems.Events;

public sealed class PlayerEventRegistry(IPlayerEntity player, IJobScheduler jobScheduler)
    : EntityEventRegistry<IPlayerEntity>(player)
{
    public SafeLogoutCountdownEvent SafeLogoutCountdown { get; } =
        new(LOGOUT_WAIT_SECONDS, LOGOUT_COMBAT_WAIT_SECONDS, LOGOUT_COMBAT_GRACE_PERIOD_SECONDS, jobScheduler);

    public OneShotEvent<IPlayerEntity> AutoRespawnInTown { get; } = new(PlayerAutoRespawnDelay,
        target => {
            if (target.Dead) target.Respawn(true);
        });

    public override void Dispose()
    {
        Cancel(SafeLogoutCountdown);
        Cancel(AutoRespawnInTown);
        base.Dispose();
    }
}
