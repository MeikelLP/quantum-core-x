using System.Drawing;
using QuantumCore.Game.Drops;

namespace QuantumCore.Game.Extensions;

public class GameOptions
{
    /// <summary>
    /// Contains the in-game shop webpage address
    /// </summary>
    public string InGameShop { get; set; } = "https://example.com/";

    /// <summary>
    /// Contains the starting locations for each empire.
    /// <remarks>Index 0 will always contain a invalid empire coordinates</remarks>
    /// </summary>
    public IReadOnlyList<Point> Empire { get; set; } = new List<Point>();

    public DropOptions Drops { get; set; } = new DropOptions();
}

public class DropOptions
{
    /// <summary>
    /// Contains the delta chances for normal and boss monsters
    /// <remarks>Delta chance is applied in auxiliary item drop calculations in basis of the monster level that got killed.
    /// Check <see cref="QuantumCore.Game.Services.DropProvider"/> for implementation details</remarks>
    /// </summary>
    public DeltaChances Delta { get; set; } = new DeltaChances();
    
    /// <summary>
    /// Contains metin stone info regarding spirit stone chances and rank level chances (+0,+1,+2...)
    /// </summary>
    public IReadOnlyList<MetinStoneDrop> MetinStones { get; set; } = new List<MetinStoneDrop>();

    /// <summary>
    /// Contains the spirit stone ids (at +0) that are used in metin drops
    /// </summary>
    public IReadOnlyList<uint> SpiritStones { get; set; } = new List<uint>();
    
}

public class DeltaChances
{
    public IReadOnlyList<uint> Boss { get; set; }
    public IReadOnlyList<uint> Normal { get; set; }
}
