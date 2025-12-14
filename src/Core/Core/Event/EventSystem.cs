// using Prometheus; // TODO

namespace QuantumCore.Core.Event;

public class EventSystem
{
    private static readonly Dictionary<long, Event> PendingEvents = new();
    private static long _nextEventId = 1;

    private static readonly List<long> Remove = new();
        
    // private static readonly Gauge EventsGauge = Metrics.CreateGauge("events", "Currently queued events");

    /// <summary>
    /// Updates all pending events and call them if needed.
    /// This method isn't thread safe and should be only called once each update cycle.
    /// </summary>
    /// <param name="elapsedTime">Elapsed time since last update call in milliseconds</param>
    public static void Update(double elapsedTime)
    {
        lock (PendingEvents)
        {
            foreach (var (id, evt) in PendingEvents)
            {
                evt.Time -= (int)elapsedTime;
                if (evt.Time > 0)
                {
                    continue;
                }

                var timeout = evt.Callback();
                if (timeout == 0)
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
    /// If the function returns 0 the event is cancelled, otherwise it will be executed again after the new returned timeout
    /// </param>
    /// <param name="timeout">
    /// Time in milliseconds until the event is executed
    /// </param>
    public static void EnqueueEvent(Func<int> callback, int timeout)
    {
        lock (PendingEvents)
        {
            var id = _nextEventId++;
            var evt = new Event(id, timeout) { Callback = callback };
            PendingEvents[id] = evt;
        }
    }
        
    /// <summary>
    /// Cancels the given event for further execution.
    /// </summary>
    /// <param name="eventId">Event ID to cancel</param>
    public static void CancelEvent(long eventId)
    {
        lock (PendingEvents)
        {
            PendingEvents.Remove(eventId);
        }
    }
}
