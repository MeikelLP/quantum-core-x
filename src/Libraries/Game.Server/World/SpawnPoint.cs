using System.Runtime.Serialization;

namespace QuantumCore.Game.World
{
    public enum ESpawnPointType
    {
        [EnumMember(Value = "g")] Group,
        [EnumMember(Value = "m")] Monster,
        [EnumMember(Value = "e")] Exception,
        [EnumMember(Value = "r")] GroupCollection,
        [EnumMember(Value = "s")] Special
    }

    /// <summary>
    /// Spawn point directions follows the compass rose with counter-clockwise increments.
    /// </summary>
    public enum ESpawnPointDirection : byte
    {
        Random = 0,
        South = 1,
        SouthEast = 2,
        East = 3,
        NorthEast = 4,
        North = 5,
        NorthWest = 6,
        West = 7,
        SouthWest = 8
    }

    public class SpawnPoint
    {
        public ESpawnPointType Type { get; set; }
        public bool IsAggressive { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int RangeX { get; set; }
        public int RangeY { get; set; }
        public ESpawnPointDirection Direction { get; set; }
        public int RespawnTime { get; set; }
        public List<int> Groups { get; } = new List<int>();
        public uint Monster { get; set; }
        public MonsterGroup? CurrentGroup { get; set; }
        public short Chance { get; set; }
        public short MaxAmount { get; set; }
    }
}