using System.Drawing;

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
}
