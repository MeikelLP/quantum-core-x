namespace QuantumCore.API.Core.Timekeeping;

/// <summary>
/// Wraps the builtin <see cref="TimeProvider"/> to expose a monotonic server clock anchored at construction time.
/// </summary>
public sealed class ServerClock
{
    private readonly TimeProvider _provider;
    private readonly long _originTimestampRaw;

    public ServerClock(TimeProvider provider)
    {
        _provider = provider;
        _originTimestampRaw = provider.GetTimestamp();
    }

    /// <summary>
    /// Current monotonic server timestamp.
    /// </summary>
    public ServerTimestamp Now => new(_provider.GetTimestamp());

    /// <summary>
    /// Elapsed time since the clock origin.
    /// </summary>
    public TimeSpan Elapsed() => _provider.GetElapsedTime(_originTimestampRaw);

    public TimeSpan ElapsedBetween(ServerTimestamp from, ServerTimestamp to) =>
        _provider.GetElapsedTime(from.Stamp, to.Stamp);

    /// <summary>
    /// Converts a timestamp into elapsed time since this clock's origin.
    /// </summary>
    public TimeSpan ElapsedAt(ServerTimestamp timestamp) =>
        _provider.GetElapsedTime(_originTimestampRaw, timestamp.Stamp);

    /// <summary>
    /// Builds a timestamp that is <paramref name="delta"/> after <paramref name="timestamp"/>.
    /// </summary>
    public ServerTimestamp Advance(ServerTimestamp timestamp, TimeSpan delta) =>
        new(timestamp.Stamp + ToRawTimestampDelta(delta));

    public ServerTimestamp Advance(TimeSpan delta) =>
        new(_provider.GetTimestamp() + ToRawTimestampDelta(delta));

    /// <summary>
    /// Builds a timestamp that is <paramref name="delta"/> before <paramref name="timestamp"/>.
    /// </summary>
    public ServerTimestamp Rewind(ServerTimestamp timestamp, TimeSpan delta) =>
        new(timestamp.Stamp - ToRawTimestampDelta(delta));

    public ServerTimestamp Rewind(TimeSpan delta) =>
        new(_provider.GetTimestamp() - ToRawTimestampDelta(delta));

    /// <summary>
    /// Builds a timestamp located <paramref name="elapsed"/> after this clock's origin.
    /// </summary>
    public ServerTimestamp AtElapsed(TimeSpan elapsed) =>
        new(_originTimestampRaw + ToRawTimestampDelta(elapsed));

    private long ToRawTimestampDelta(TimeSpan delta) =>
        (long)(delta.Ticks / (double)TimeSpan.TicksPerSecond * _provider.TimestampFrequency);

    public TimeProvider GetTimeProvider() => _provider;
}
