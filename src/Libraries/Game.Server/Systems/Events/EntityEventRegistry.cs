using QuantumCore.API.Game.World;
using QuantumCore.API.Systems.Events;

namespace QuantumCore.Game.Systems.Events;

public class EntityEventRegistry<TEntity> : EntityEventRegistryBase where TEntity : IEntity
{
    private readonly TEntity _entity;

    protected EntityEventRegistry(TEntity entity) : base(entity)
    {
        _entity = entity;
    }

    public long Schedule(ISchedulable<TEntity> schedulable)
    {
        Cancel(schedulable);

        var eventId = schedulable.EnqueueEvent(_entity);
        schedulable.EventId = eventId;
        return eventId;
    }

    public long Schedule<TArgs>(ISchedulable<TEntity, TArgs> schedulable, TArgs args)
    {
        Cancel(schedulable);

        var eventId = schedulable.EnqueueEvent(_entity, args);
        schedulable.EventId = eventId;
        return eventId;
    }

    public bool Cancel(ISchedulable<TEntity> schedulable) => schedulable.Cancel();

    public bool Cancel<TArgs>(ISchedulable<TEntity, TArgs> schedulable) => schedulable.Cancel();
}
