using QuantumCore.API.Game.World;
using QuantumCore.API.Systems.Events;
using QuantumCore.Core.Event;
using Serilog;

namespace QuantumCore.Game.Systems.Events;

// convention: the callback returns false if retry is needed
public sealed class RetryableEvent<TEntity, TArgs>(
    TimeSpan retryDelay,
    Func<TEntity, TArgs, bool> callback,
    int maxRetries = 99)
    : ISchedulable<TEntity, TArgs> where TEntity : IEntity
{
    public long? EventId { get; set; }

    public long EnqueueEvent(TEntity entity, TArgs arg)
    {
        var remainingRetries = maxRetries;
        return EventSystem.EnqueueEvent(() =>
        {
            var succeeded = callback(entity, arg);
            if (succeeded)
            {
                EventId = null;
                return TimeSpan.Zero;
            }

            if (remainingRetries-- > 0)
            {
                return retryDelay;
            }

            Log.Error("Retryable event id {Id} for entity {Entity}: exhausted all {MaxRetries} retries",
                EventId, entity, maxRetries);
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
}
