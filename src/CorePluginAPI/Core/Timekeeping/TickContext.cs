namespace QuantumCore.API.Core.Timekeeping;

public readonly struct TickContext(
    ServerClock clock,
    TimeSpan delta,
    ServerTimestamp timestamp)
{
    // Used as a sentinel for "no past event" when comparing deltas
    // Must stay tiny compared to TimeSpan.MaxValue to avoid overflow if used in arithmetic
    private static readonly TimeSpan EffectivelyInfinite = TimeSpan.FromDays(365 * 100);

    public TimeSpan Delta { get; } = delta;
    public ServerTimestamp Timestamp { get; } = timestamp;

    public TimeSpan ElapsedSince(ServerTimestamp past) => clock.ElapsedBetween(past, Timestamp);

    public TimeSpan ElapsedSince(ServerTimestamp? past) =>
        past.HasValue ? ElapsedSince(past.Value) : EffectivelyInfinite;

    public TimeSpan TotalElapsed => clock.ElapsedAt(Timestamp);

    public ServerTimestamp Advance(TimeSpan delta) => clock.Advance(Timestamp, delta);
    public ServerTimestamp Rewind(TimeSpan delta) => clock.Rewind(Timestamp, delta);
}
