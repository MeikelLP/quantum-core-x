namespace QuantumCore.API.Systems.Events;

// Strongly typed wrapper over the raw event ID; null id signifies unscheduled
public interface IEventSlot
{
    long? EventId { get; set; }
}
