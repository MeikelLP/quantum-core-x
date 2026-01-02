using QuantumCore.API.Game.World;
using QuantumCore.API.Systems.Events;
using QuantumCore.Core.Event;

namespace QuantumCore.Game.Systems.Events;

public sealed class OneShotEvent<TEntity>(TimeSpan delay, Action<TEntity> callback) : ISchedulable<TEntity>
    where TEntity : IEntity
{
    public long? EventId { get; set; }

    public long EnqueueEvent(TEntity entity)
    {
        return EventSystem.EnqueueEvent(() =>
        {
            EventId = null;
            callback(entity);
            return TimeSpan.Zero;
        }, delay);
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
}
