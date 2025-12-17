using QuantumCore.API.Core.Timekeeping;

namespace QuantumCore.Core.Timekeeping;

/// <summary>
/// Deterministic provider for tests; time only moves when manually advanced, thread-safe.
/// </summary>
public sealed class ManualTimeProvider : ITimeProvider
{
    private long _ticks;

    public ServerTimestamp Now => new(TimeSpan.FromTicks(Interlocked.Read(ref _ticks)));

    public void Advance(TimeSpan delta)
    {
        if (delta < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(delta), "Cannot rewind time");
        }

        Interlocked.Add(ref _ticks, delta.Ticks);
    }

    public void Reset(TimeSpan? start = null)
    {
        Interlocked.Exchange(ref _ticks, start?.Ticks ?? 0);
    }

    public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        if (delay > TimeSpan.Zero)
        {
            Advance(delay);
        }

        return Task.CompletedTask;
    }
}
