namespace QuantumCore.Core.Event;

public class Event
{
    public long Id { get; }
    public required Func<int> Callback { get; init; }
    public int Time { get; set; }

    public Event(long id, int timeout)
    {
        Id = id;
        Time = timeout;
    }
}