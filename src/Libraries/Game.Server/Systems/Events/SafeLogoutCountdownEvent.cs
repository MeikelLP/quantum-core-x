using QuantumCore.API.Core.Event;
using QuantumCore.API.Core.Timekeeping;
using QuantumCore.API.Game.World;
using QuantumCore.API.Systems.Events;
using QuantumCore.Core.Event;
using Serilog;

namespace QuantumCore.Game.Systems.Events;

public sealed class SafeLogoutCountdownEvent(
    int normalWaitSeconds,
    int combatWaitSeconds,
    int combatGraceSeconds,
    IJobScheduler scheduler)
    : ISchedulable<IPlayerEntity, SafeLogoutCountdownEvent.Args>
{
    public long? EventId { get; set; }

    public readonly record struct Args(
        string CountdownMessageTemplate,
        Func<Task> OnCompleteAsync);

    public long EnqueueEvent(IPlayerEntity player, Args args)
    {
        var schedulingTime = player.Connection.Server.Clock.Now;
        var remainingSeconds = ComputeCountdownSeconds(player, schedulingTime);

        return EventSystem.EnqueueEvent(() =>
        {
            if (LastCombatActivity(player) > schedulingTime)
            {
                player.SendChatInfo("In combat, cancelled.");
                EventId = null;
                return TimeSpan.Zero;
            }

            if (remainingSeconds > 0)
            {
                player.SendChatInfo(string.Format(args.CountdownMessageTemplate, remainingSeconds));
                remainingSeconds--;

                return TimeSpan.FromSeconds(1);
            }

            scheduler.Schedule(async () =>
            {
                try
                {
                    await args.OnCompleteAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[{CountdownEvent}] Completion callback threw exception for player {Player}",
                        nameof(SafeLogoutCountdownEvent), player);
                }
            });
            EventId = null;
            return TimeSpan.Zero;
        }, TimeSpan.Zero);
    }

    public bool Cancel()
    {
        if (!EventId.HasValue)
        {
            return false;
        }

        EventSystem.CancelEvent(EventId.Value);
        EventId = null;
        return true;
    }

    private int ComputeCountdownSeconds(IPlayerEntity player, ServerTimestamp schedulingTime)
    {
        var clock = player.Connection.Server.Clock;
        if (LastCombatActivity(player) is { } lastCombatTime)
        {
            var combatExpirationTime = clock.Advance(lastCombatTime, TimeSpan.FromSeconds(combatGraceSeconds));
            if (schedulingTime < combatExpirationTime)
            {
                return combatWaitSeconds;
            }
        } 
        
        return normalWaitSeconds;
    }

    private static ServerTimestamp? LastCombatActivity(IPlayerEntity player)
    {
        // TODO: extend for trade, shop open, other interactions
        return player.Timeline.LatestOf(
            PlayerTimestampKind.DAMAGE_DEALT,
            PlayerTimestampKind.DAMAGE_TAKEN,
            PlayerTimestampKind.USED_SKILL); 
    }
}
