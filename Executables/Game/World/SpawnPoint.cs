using System.Runtime.Serialization;

namespace QuantumCore.Game.World
{
    public enum ESpawnPointType
    {
        [EnumMember(Value = "g")]
        Group,
        [EnumMember(Value = "m")]
        Monster,
        [EnumMember(Value = "e")]
        Exception,
        [EnumMember(Value = "r")]
        GroupCollection,
        [EnumMember(Value = "s")]
        Special
    }

    public class SpawnPoint
    {
        public ESpawnPointType Type { get; set; }
        public bool IsAggressive { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int RangeX { get; set; }
        public int RangeY { get; set; }
        public int Direction { get; set; }
        public int RespawnTime { get; set; }
        public List<int> Groups { get; } = new List<int>();
        public uint Monster { get; set; }
        public MonsterGroup CurrentGroup { get; set; }
        public short Chance { get; set; }
        public short MaxAmount { get; set; }
    }
}
