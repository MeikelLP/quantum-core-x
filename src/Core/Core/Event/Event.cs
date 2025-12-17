namespace QuantumCore.Core.Event;

public class Event
{
    public long Id { get; }
    public required Func<TimeSpan> Callback { get; init; }
    public TimeSpan Time { get; set; }

    public Event(long id, TimeSpan timeout)
    {
        Id = id;
        Time = timeout;
    }
}
