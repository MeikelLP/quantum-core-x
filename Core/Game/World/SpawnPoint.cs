using System.Collections.Generic;
using Tomlyn.Model;

namespace QuantumCore.Game.World
{
    public enum ESpawnPointType
    {
        Group
    }
    
    public class SpawnPoint
    {
        public ESpawnPointType Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Range { get; set; }
        public int RespawnTime { get; set; }
        public List<int> Groups { get; } = new List<int>();
        public MonsterGroup CurrentGroup { get; set; }

        public static SpawnPoint FromToml(TomlTable toml)
        {
            var sp = new SpawnPoint();
            
            var type = toml["type"] as string;
            switch (type)
            {
                case "group":
                    sp.Type = ESpawnPointType.Group;
                    var groups = toml["groups"] as TomlArray;
                    if (groups != null)
                    {
                        foreach (var groupId in groups)
                        {
                            sp.Groups.Add((int)(groupId as long? ?? 0));
                        }
                    }

                    break;
            }
                
            sp.X = (int)(toml["x"] as long? ?? 0);
            sp.Y = (int)(toml["y"] as long? ?? 0);
            sp.Range = (int)(toml["range"] as long? ?? 0);
            sp.RespawnTime = (int)(toml["respawnTime"] as long? ?? 0);

            return sp;
        }
    }
}