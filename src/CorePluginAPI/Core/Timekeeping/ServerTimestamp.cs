namespace QuantumCore.API.Core.Timekeeping;

/// <summary>
/// Strongly typed wrapper for monotonic timestamps returned by <see cref="TimeProvider.GetTimestamp"/>.
/// The underlying value is opaque and must be interpreted via a <see cref="ServerClock"/>.
/// </summary>
public readonly record struct ServerTimestamp(long Stamp)
    : IComparable<ServerTimestamp>
{
    public int CompareTo(ServerTimestamp other) => Stamp.CompareTo(other.Stamp);

    public static bool operator <(ServerTimestamp left, ServerTimestamp right) => left.Stamp < right.Stamp;
    public static bool operator <=(ServerTimestamp left, ServerTimestamp right) => left.Stamp <= right.Stamp;
    public static bool operator >(ServerTimestamp left, ServerTimestamp right) => left.Stamp > right.Stamp;
    public static bool operator >=(ServerTimestamp left, ServerTimestamp right) => left.Stamp >= right.Stamp;

    public static ServerTimestamp Min(ServerTimestamp a, ServerTimestamp b) => a.Stamp <= b.Stamp ? a : b;
    public static ServerTimestamp Max(ServerTimestamp a, ServerTimestamp b) => a.Stamp >= b.Stamp ? a : b;
}
