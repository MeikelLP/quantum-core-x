namespace QuantumCore.API.Core.Timekeeping;

public interface ITimeProvider
{
    ServerTimestamp Now { get; }
    Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default);
}
