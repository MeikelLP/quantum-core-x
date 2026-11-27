using System.Collections.Immutable;

namespace QuantumCore.API.Game.World;

public class SpawnMember
{
    public uint Id { get; init; }
}

public class SpawnGroupCollectionMember
{
    public uint Id { get; init; }

    /// <summary>
    /// 0 to 1 probability of spawning
    /// </summary>
    public float Probability { get; set; }
}

public class SpawnGroup
{
    public uint Id { get; set; }
    public string Name { get; set; } = "";
    public uint Leader { get; set; }
    public ImmutableArray<SpawnMember> Members { get; init; } = [];
}

public class SpawnGroupCollection
{
    public uint Id { get; set; }
    public string Name { get; set; } = "";
    public ImmutableArray<SpawnGroupCollectionMember> Groups { get; init; } = [];
}