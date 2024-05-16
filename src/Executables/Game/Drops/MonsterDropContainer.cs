using QuantumCore.Core.Utils;

namespace QuantumCore.Game.Drops;

public class MonsterDropContainer { }

public record MetinStoneDrop(int MonsterProtoId, int DropChance, int[] RankChance);

public class MonsterItemGroup : MonsterDropContainer
{
    public class Drop
    {
        public uint ItemProtoId { get; init; }
        public uint Amount { get; init; }
        public uint Chance { get; set; }
    }
    
    public uint MonsterProtoId { get; init; }
    public uint MinKillCount { get; init; }
    public List<Drop> Drops { get; init; } = new();
    public List<uint> Probabilities { get; init; } = new();
    
    public void AddDrop(uint itemProtoId, uint count, uint dropChance, uint rareDropChance)
    {
        Probabilities.Add(dropChance);
        Drops.Add(new Drop
        {
            ItemProtoId = itemProtoId,
            Amount = count,
            Chance = rareDropChance
        });
    }

    public bool IsEmpty => Probabilities.Count == 0;

    public int GetOneIndex()
    {
        var n = CoreRandom.GenerateInt32(0, Probabilities.Count + 1);
        var lowerBound = 0;
        // find first element not before n
        for (var i = 0; i < Probabilities.Count; i++)
        {
            if (Probabilities[i] >= n)
            {
                lowerBound = i;
                break;
            }
        }

        var distance = Probabilities.Count - lowerBound;
        return distance;
    }

    public Drop? GetDrop()
    {
        if (IsEmpty)
        {
            return null;
        }

        var index = GetOneIndex();
        return Drops[index];
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
    public List<Drop> Drops { get; init; } = [];
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
    public List<Drop> Drops { get; init; } = [];
}
