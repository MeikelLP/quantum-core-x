namespace QuantumCore.API.Game.World
{
    public class SpawnMember
    {
        public uint Id { get; set; }
    }
    public class SpawnGroupCollectionMember
    {
        public uint Id { get; set; }
        public byte Amount { get; set; }
    }

    public class SpawnGroup
    {
        public uint Id { get; set; }
        public string Name { get; set; } = "";
        public uint Leader { get; set; }
        public List<SpawnMember> Members { get; } = new List<SpawnMember>();
    }

    public class SpawnGroupCollection
    {
        public uint Id { get; set; }
        public string Name { get; set; } = "";
        public List<SpawnGroupCollectionMember> Groups { get; } = new List<SpawnGroupCollectionMember>();
    }
}
