using System.Collections.Generic;
using Tomlyn.Model;

namespace QuantumCore.Game.World
{
    public class SpawnMember
    {
        public uint Id { get; set; }

        public static SpawnMember FromToml(TomlTable toml)
        {
            return new SpawnMember {
                Id = (uint)(toml["id"] as long? ?? 0)
            };
        }
    }
    
    public class SpawnGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<SpawnMember> Members { get; } = new List<SpawnMember>();

        public static SpawnGroup FromToml(TomlTable toml)
        {
            var sg = new SpawnGroup();
            if (toml["member"] is TomlTableArray members)
            {
                foreach (var member in members)
                {
                    sg.Members.Add(SpawnMember.FromToml(member));
                }
            }

            sg.Id = (int) (toml["id"] as long? ?? 0);
            sg.Name = toml.Keys.Contains("name") ? toml["name"] as string : "";

            return sg;
        }
    }
}