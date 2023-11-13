using System.Collections.Immutable;

namespace QuantumCore.Game.Services;

public record struct MonsterDropEntry(uint ItemProtoId, float Chance, uint MinLevel = 0, uint MinKillCount = 0, byte Amount = 1);
public record struct CommonDropEntry(byte MinLevel, byte MaxLevel, uint ItemProtoId, float Chance);

public interface IDropProvider
{
    /// <summary>
    /// Gets drops found for the given mob.
    /// </summary>
    /// <param name="monsterProtoId">For what monster shall we lookup drops</param>
    /// <returns>The amount of drops that have written to the array</returns>
    ImmutableArray<MonsterDropEntry> GetDropsForMob(uint monsterProtoId);

    ImmutableArray<CommonDropEntry> CommonDrops { get; }

    Task LoadAsync(CancellationToken cancellationToken = default);
}
