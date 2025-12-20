using EnumsNET;

namespace QuantumCore.API.Core.Timekeeping;

/// <summary>
/// Simple enum-indexed storage for optional server timestamps.
/// Made thread-safe using <see cref="System.Threading.Lock"/>.
/// </summary>
public sealed class TimestampRegistry<TKind> where TKind : struct, Enum
{
    private static readonly int MaxEnumValue = Enums.GetValues<TKind>().Select(Enums.ToInt32).Max();

    private readonly Lock _lock = new();
    private readonly ServerTimestamp?[] _slots = new ServerTimestamp?[MaxEnumValue + 1];

    public ServerTimestamp? this[TKind kind]
    {
        get => Get(kind);
        set
        {
            if (value.HasValue)
            {
                Mark(kind, value.Value);
            }
            else
            {
                Clear(kind);
            }
        }
    }
    
    public void Mark(TKind kind, ServerTimestamp timestamp)
    {
        lock (_lock)
        {
            _slots[Enums.ToInt32(kind)] = timestamp;
        }
    }

    public void Clear(TKind kind)
    {
        lock (_lock)
        {
            _slots[Enums.ToInt32(kind)] = null;
        }
    }

    public bool UpdateIfElapsed(TickContext ctx, TKind kind, TimeSpan minimumElapsed)
    {
        // need lock for transactional update (check-then-act)
        lock (_lock)
        {
            var ts = _slots[Enums.ToInt32(kind)];
            if (ts.HasValue)
            {
                var tickTimestampOutsideGracePeriod = ctx.ElapsedSince(ts) > minimumElapsed;
                if (tickTimestampOutsideGracePeriod)
                {
                    _slots[Enums.ToInt32(kind)] = ctx.Timestamp;
                    return true;
                }
            }
            else
            {
                _slots[Enums.ToInt32(kind)] = ctx.Timestamp;
            }

            return false;
        }
    }

    public ServerTimestamp? Get(TKind kind)
    {
        lock (_lock)
        {
            return _slots[Enums.ToInt32(kind)];
        }
    }
    
    public ServerTimestamp? LatestOf(params TKind[] kinds)
    {
        lock (_lock)
        {
            ServerTimestamp? latest = null;
            foreach (var kind in kinds)
            {
                var ts = _slots[Enums.ToInt32(kind)];
                if (ts > latest)
                {
                    latest = ts;
                }
            }

            return latest;
        }
    }
}
