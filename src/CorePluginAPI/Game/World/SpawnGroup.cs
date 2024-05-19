namespace QuantumCore.API.Game.World
{
    public class SpawnMember
    {
        public uint Id { get; init; }
    }
    public class SpawnGroupCollectionMember
    {
        public uint Id { get; init; }
        public byte Amount { get; init; }
    }

    public class SpawnGroup
    {
        public uint Id { get; set; }
        public string Name { get; set; } = "";
        public uint Leader { get; set; }
        public List<SpawnMember> Members { get; init; } = [];
    }

    public class SpawnGroupCollection
    {
        public uint Id { get; set; }
        public string Name { get; set; } = "";
        public List<SpawnGroupCollectionMember> Groups { get; init; } = [];
    }
}
