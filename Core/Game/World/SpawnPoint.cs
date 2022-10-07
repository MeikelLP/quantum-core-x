using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Tomlyn.Model;

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

        public static SpawnPoint FromToml(ILogger logger, TomlTable toml)
        {
            var sp = new SpawnPoint();
            
            var type = toml["type"] as string;
            switch (type)
            {
                case "group":
                    sp.Type = ESpawnPointType.Group;
                    if (toml["groups"] is TomlArray groups)
                    {
                        foreach (var groupId in groups)
                        {
                            sp.Groups.Add((int)(groupId as long? ?? 0));
                        }
                    }

                    break;
                case "monster":
                    sp.Type = ESpawnPointType.Monster;
                    sp.Monster = (uint) (toml["monster"] as long? ?? 0);
                    sp.Direction = (int)(toml["direction"] as long? ?? 0);
                    break;
                default:
                    logger.LogWarning("Unknown spawn type {Type}", type);
                    break;
            }

            if (toml.ContainsKey("randomPosition") && toml["randomPosition"] as bool? == true)
            {
                sp.RandomPosition = true;
            }
            else
            {
                sp.X = (int) (toml["x"] as long? ?? 0);
                sp.Y = (int) (toml["y"] as long? ?? 0);
                sp.Range = (int)(toml["range"] as long? ?? 0);
            }
            
            sp.RespawnTime = (int)(toml["respawnTime"] as long? ?? 0);

            return sp;
        }
    }
}