using System.Collections.Immutable;
using QuantumCore.Core.Utils;

namespace QuantumCore.Game.Drops;

public class MonsterDropContainer
{
}

public record MetinStoneDrop(int MonsterProtoId, int DropChance, ImmutableArray<int> RankChance);

public class MonsterItemGroup : MonsterDropContainer
{
    private readonly List<Drop> _drops = [];
    private readonly List<uint> _probabilities = [];

    public class Drop
    {
        public uint ItemProtoId { get; init; }
        public uint Amount { get; init; }
        public uint Chance { get; set; }
    }

    public uint MonsterProtoId { get; init; }
    public uint MinKillCount { get; init; }

    public ImmutableArray<Drop> Drops
    {
        get => [.. _drops];
        init => _drops = [.. value];
    }

    public ImmutableArray<uint> Probabilities
    {
        get => [.. _probabilities];
        init => _probabilities = [.. value];
    }

    public void AddDrop(uint itemProtoId, uint count, uint dropChance, uint rareDropChance)
    {
        _probabilities.Add(dropChance);
        _drops.Add(new Drop
        {
            ItemProtoId = itemProtoId,
            Amount = count,
            Chance = rareDropChance
        });
    }

    public bool IsEmpty => _probabilities.Count == 0;

    public int GetOneIndex()
    {
        var n = CoreRandom.GenerateInt32(0, _probabilities.Count + 1);
        var lowerBound = 0;
        // find first element not before n
        for (var i = 0; i < _probabilities.Count; i++)
        {
            if (_probabilities[i] >= n)
            {
                lowerBound = i;
                break;
            }
        }

        var distance = _probabilities.Count - lowerBound;
        return distance;
    }

    public Drop? GetDrop()
    {
        if (IsEmpty)
        {
            return null;
        }

        var index = GetOneIndex();
        return _drops[index];
    }
}

public class DropItemGroup : MonsterDropContainer
{
    public class Drop
    {
        public uint ItemProtoId { get; init; }
        public uint Amount { get; init; }
        public float Chance { get; init; }
    }

    public uint MonsterProtoId { get; init; }
    public ImmutableArray<Drop> Drops { get; init; } = [];
}

public class LevelItemGroup : MonsterDropContainer
{
    public class Drop
    {
        public uint ItemProtoId { get; init; }
        public uint Amount { get; init; }
        public float Chance { get; init; }
    }

    public uint LevelLimit { get; init; }
    public ImmutableArray<Drop> Drops { get; init; } = [];
}