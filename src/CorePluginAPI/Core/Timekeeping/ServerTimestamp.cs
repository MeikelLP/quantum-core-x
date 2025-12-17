using System.Diagnostics.CodeAnalysis;

namespace QuantumCore.API.Core.Timekeeping;

/// <summary>
/// Strongly typed wrapper for monotonically increasing server timestamps.
/// </summary>
public readonly struct ServerTimestamp(TimeSpan value)
    : IEquatable<ServerTimestamp>, IComparable<ServerTimestamp>
{
    private readonly TimeSpan _value = value;

    public TimeSpan Value => _value;
    public double TotalMilliseconds => _value.TotalMilliseconds;
    public double TotalSeconds => _value.TotalSeconds;

    public TimeSpan Since(ServerTimestamp pastEvent) => this - pastEvent;

    // Used as a sentinel for "no past event" when comparing deltas
    // Must stay tiny compared to TimeSpan.MaxValue to avoid overflow if used in arithmetic
    private static readonly TimeSpan EffectivelyInfinite = TimeSpan.FromDays(365 * 100);

    public TimeSpan Since(ServerTimestamp? pastEvent) => 
        pastEvent.HasValue ? Since(pastEvent.Value) : EffectivelyInfinite; 

    public int CompareTo(ServerTimestamp other) => _value.CompareTo(other._value);

    public bool Equals(ServerTimestamp other) => _value.Equals(other._value);
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ServerTimestamp other && Equals(other);
    public override int GetHashCode() => _value.GetHashCode();
    public override string ToString() => _value.ToString();

    public static ServerTimestamp operator +(ServerTimestamp timestamp, TimeSpan delta) => 
            new(timestamp._value + delta);

    public static ServerTimestamp operator +(TimeSpan delta, ServerTimestamp timestamp) => 
            new(delta + timestamp._value);

    public static ServerTimestamp operator -(ServerTimestamp timestamp, TimeSpan delta) => 
            new(timestamp._value - delta);

    public static TimeSpan operator -(ServerTimestamp left, ServerTimestamp right) => left._value - right._value;

    public static bool operator >(ServerTimestamp left, ServerTimestamp right) => left._value > right._value;
    public static bool operator <(ServerTimestamp left, ServerTimestamp right) => left._value < right._value;
    public static bool operator >=(ServerTimestamp left, ServerTimestamp right) => left._value >= right._value;
    public static bool operator <=(ServerTimestamp left, ServerTimestamp right) => left._value <= right._value;

    public static bool operator ==(ServerTimestamp left, ServerTimestamp right) => left.Equals(right);
    public static bool operator !=(ServerTimestamp left, ServerTimestamp right) => !left.Equals(right);
}
