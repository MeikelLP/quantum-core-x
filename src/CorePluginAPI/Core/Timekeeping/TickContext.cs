namespace QuantumCore.API.Core.Timekeeping;

public readonly record struct TickContext(
    TimeSpan Elapsed,
    ServerTimestamp Now);
