// using Prometheus; // TODO

using QuantumCore.API.Core.Timekeeping;

namespace QuantumCore.Core.Event;

public class EventSystem
{
    private static readonly Lock Lock = new();

    private static readonly Dictionary<long, Event> PendingEvents = new();
    private static long NextEventId = 1;

    private static readonly List<long> Remove = new();
        
    // private static readonly Gauge EventsGauge = Metrics.CreateGauge("events", "Currently queued events");

    /// <summary>
    /// Updates all pending events and call them if needed.
    /// This method isn't thread safe and should be only called once each update cycle.
    /// </summary>
    /// <param name="ctx">Elapsed time since last update call and current timestamp</param>
    public static void Update(TickContext ctx)
    {
        lock (Lock)
        {
            foreach (var (id, evt) in PendingEvents)
            {
                evt.Time -= ctx.Delta;
                if (evt.Time > TimeSpan.Zero)
                {
                    continue;
                }

                var timeout = evt.Callback();
                if (timeout <= TimeSpan.Zero)
                {
                    Remove.Add(id);
                }
                else
                {
                    evt.Time += timeout;
                }
            }

            foreach (var r in Remove)
            {
                PendingEvents.Remove(r);
            }
            Remove.Clear();

            // EventsGauge.Set(PendingEvents.Count);
        }
    }
        
    /// <summary>
    /// Enqueues the given function to be executed in the event loop.
    /// </summary>
    /// <param name="callback">
    /// Function to be called after the given timeout.
    /// If the function returns <see cref="TimeSpan.Zero"/> the event is canceled, otherwise it will be executed again
    /// after the new returned timeout.
    /// </param>
    /// <param name="timeout">
    /// TimeSpan until the event is executed
    /// </param>
    /// <returns>
    /// The enqueued event ID to be used if canceling.
    /// </returns>
    public static long EnqueueEvent(Func<TimeSpan> callback, TimeSpan timeout)
    {
        lock (Lock)
        {
            var id = NextEventId++;
            var evt = new Event(id, timeout) { Callback = callback };
            PendingEvents[id] = evt;

            return id;
        }
    }
        
    /// <summary>
    /// Cancels the given event for further execution.
    /// </summary>
    /// <param name="eventId">Event ID to cancel</param>
    public static void CancelEvent(long eventId)
    {
        lock (Lock)
        {
            PendingEvents.Remove(eventId);
        }
    }
}
