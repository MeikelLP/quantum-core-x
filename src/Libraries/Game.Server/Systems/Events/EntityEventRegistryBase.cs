using QuantumCore.API.Game.World;
using QuantumCore.API.Systems.Events;
using QuantumCore.Game.Constants;

namespace QuantumCore.Game.Systems.Events;

/// <summary>
/// This class (and its inheritors) is created to help with readability
/// by declaring all specific event types as properties or "event slots", therefore
/// avoiding bloat in the main entity classes, centralizing scheduling configuration,
/// and discouraging haphazardly scheduling random types of events throughout the code.
/// </summary>
public abstract class EntityEventRegistryBase : IDisposable
{
    private readonly IEntity _entity;
    
    protected EntityEventRegistryBase(IEntity entity)
    {
        _entity = entity;
    }

    public OneShotEvent<IEntity> KnockoutDeath { get; } = new(SchedulingConstants.KnockoutToDeathDelay,
        target => target.Die());

    public static bool IsScheduled(IEventSlot eventSlot) => eventSlot.EventId.HasValue;

    public long Schedule(ISchedulable<IEntity> schedulable)
    {
        schedulable.Cancel();

        var eventId = schedulable.EnqueueEvent(_entity);
        schedulable.EventId = eventId;
        return eventId;
    }

    public static bool Cancel(ISchedulable<IEntity> schedulable) => schedulable.Cancel();

    public virtual void Dispose()
    {
        KnockoutDeath.Cancel();
    }

}
