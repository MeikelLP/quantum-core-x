using System.Collections.Generic;

namespace QuantumCore.API.Game.World
{
    public class SpawnMember
    {
        public uint Id { get; set; }
    }
    
    public class SpawnGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<SpawnMember> Members { get; } = new List<SpawnMember>();
    }
}