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

    public SkillsOptions Skills { get; set; } = new SkillsOptions();

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

public class SkillsOptions
{
    /// <summary>
    /// Skill book id that is used when creating a specific skill book for a skill
    /// </summary>
    public uint GenericSkillBookId { get; set; } = 50300;

    /// <summary>
    /// Identifier for iterating over skill book ids
    /// </summary>
    public uint SkillBookStartId { get; set; } = 50400;

    /// <summary>
    /// Consumed player experience when using a skill book
    /// </summary>
    public int SkillBookNeededExperience { get; set; } = 20000;

    /// <summary>
    /// Minimum delay to wait after using a skill book
    /// </summary>
    public int SkillBookDelayMin { get; set; } = 64800;

    /// <summary>
    /// Maximum delay to wait after using a skill book
    /// </summary>
    public int SkillBookDelayMax { get; set; } = 108000;

    /// <summary>
    /// Identifier for the soul stone item
    /// </summary>
    public int SoulStoneId { get; set; } = 50513;
}