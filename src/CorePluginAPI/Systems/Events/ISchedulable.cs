using QuantumCore.API.Game.World;

namespace QuantumCore.API.Systems.Events;

public interface ISchedulable<in TEntity> : IEventSlot where TEntity : IEntity
{
    long EnqueueEvent(TEntity entity);
    bool Cancel();
}

public interface ISchedulable<in TEntity, in TArgs> : IEventSlot where TEntity : IEntity
{
    long EnqueueEvent(TEntity entity, TArgs args);
    bool Cancel();
}
