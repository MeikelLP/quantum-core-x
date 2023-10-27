namespace QuantumCore.Game.Services;

public record struct DropEntry(uint ItemProtoId, float Chance, uint MinLevel = 0, uint MinKillCount = 0, byte Amount = 1);

public interface IDropProvider
{
    /// <summary>
    /// Gets drops found for the given mob.
    /// </summary>
    /// <param name="monsterProtoId">For what monster shall we lookup drops</param>
    /// <returns>The amount of drops that have written to the array</returns>
    IReadOnlyCollection<DropEntry> GetDropsForMob(uint monsterProtoId);

    Task LoadAsync(CancellationToken cancellationToken = default);
}
