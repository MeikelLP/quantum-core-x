using QuantumCore.Core.Utils;

namespace QuantumCore.Game.Drops;

public class MonsterDropContainer
{
    
}

public class MonsterItemGroup : MonsterDropContainer
{
    public class Drop
    {
        public uint ItemProtoId { get; set; }
        public uint Amount { get; set; }
        public uint Chance { get; set; }
    }
    
    public uint MonsterProtoId { get; set; }
    public uint MinKillCount { get; set; }
    public List<Drop> Drops { get; set; } = new();
    public List<uint> Probabilities { get; set; } = new();
    
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
        for (int i = 0; i < Probabilities.Count; i++)
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
        public uint ItemProtoId { get; set; }
        public uint Amount { get; set; }
        public float Chance { get; set; }
    }
    
    public uint MonsterProtoId { get; set; }
    public List<Drop> Drops { get; set; } = new();
}

public class LevelItemGroup : MonsterDropContainer
{
    public class Drop
    {
        public uint ItemProtoId { get; set; }
        public uint Amount { get; set; }
        public float Chance { get; set; }
    }
    
    public uint LevelLimit { get; set; }
    public List<Drop> Drops { get; set; } = new();
}
