namespace QuantumCore.API.Core.Timekeeping;

public readonly record struct TickContext(
    ServerClock Clock,
    TimeSpan Delta,
    ServerTimestamp Timestamp)
{
    // Used as a sentinel for "no past event" when comparing deltas
    // Must stay tiny compared to TimeSpan.MaxValue to avoid overflow if used in arithmetic
    private static readonly TimeSpan _effectivelyInfinite = TimeSpan.FromDays(365 * 100);

    public TimeSpan Delta { get; } = Delta;
    public ServerTimestamp Timestamp { get; } = Timestamp;

    public TimeSpan ElapsedSince(ServerTimestamp past) => Clock.ElapsedBetween(past, Timestamp);

    public TimeSpan ElapsedSince(ServerTimestamp? past) =>
        past.HasValue ? ElapsedSince(past.Value) : _effectivelyInfinite;

    public TimeSpan TotalElapsed => Clock.ElapsedAt(Timestamp);

    public ServerTimestamp Advance(TimeSpan delta) => Clock.Advance(Timestamp, delta);
    public ServerTimestamp Rewind(TimeSpan delta) => Clock.Rewind(Timestamp, delta);
}