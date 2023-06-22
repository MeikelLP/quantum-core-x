namespace QuantumCore.Game.World
{
    public enum ESpawnPointType
    {
        Group,
        Monster
    }
    
    public class SpawnPoint
    {
        public ESpawnPointType Type { get; set; }
        public bool RandomPosition { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Range { get; set; }
        public int Direction { get; set; }
        public int RespawnTime { get; set; }
        public List<int> Groups { get; } = new List<int>();
        public uint Monster { get; set; }
        public MonsterGroup CurrentGroup { get; set; }
    }
}