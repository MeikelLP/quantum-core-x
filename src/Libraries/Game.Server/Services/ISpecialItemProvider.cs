using System.Collections.Immutable;

namespace QuantumCore.Game.Services;

/// <summary>
/// One possible reward of a container item, with the weight it is picked by.
/// </summary>
public record struct SpecialItemEntry(uint ItemProtoId, byte Count, uint Weight);

/// <summary>
/// The contents of container items - chests and the like - as defined by
/// <c>special_item_group.txt</c>.
/// </summary>
public interface ISpecialItemProvider
{
    /// <summary>
    /// Everything the given container can yield. Empty when the item is not a container.
    /// </summary>
    ImmutableArray<SpecialItemEntry> GetPossibleContents(uint containerItemProtoId);

    /// <summary>
    /// Picks one reward at random, weighted by the entries' weights.
    /// Null when the item is not a container or its group has no usable entry.
    /// </summary>
    SpecialItemEntry? Roll(uint containerItemProtoId);
}
