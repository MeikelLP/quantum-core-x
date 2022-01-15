using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Prometheus;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Core.API;
using QuantumCore.Core.Utils;
using QuantumCore.Game.World.Entities;
using Serilog;
using Tomlyn;
using Tomlyn.Model;

namespace QuantumCore.Game.World
{
    public class World : IWorld
    {
        private uint _vid;
        private readonly Grid<Map> _world = new Grid<Map>(0, 0);
        private readonly Dictionary<string, Map> _maps = new Dictionary<string, Map>();
        private readonly Dictionary<string, PlayerEntity> _players = new Dictionary<string, PlayerEntity>();
        private readonly Dictionary<int, SpawnGroup> _groups = new Dictionary<int, SpawnGroup>();

        private readonly Histogram _updateDuration =
            Metrics.CreateHistogram("world_update_duration_seconds", "How long did a world update took");
        private readonly Gauge _entities = Metrics.CreateGauge("entities", "Currently handles entities");
        
        public static World Instance { get; private set; }
        
        public World()
        {
            _vid = 0;
            Instance = this;
        }
        
        public void Load()
        {
            // Load groups
            var path = Path.Join("data", "groups.toml");
            if (File.Exists(path))
            {
                var toml = Toml.Parse(File.ReadAllText(path));
                var model = toml.ToModel();
                if (model["group"] is TomlTableArray groups)
                {
                    foreach (var group in groups)
                    {
                        var g = SpawnGroup.FromToml(group);
                        _groups[g.Id] = g;
                    }
                }
            }
            
            // Regex for parsing lines in the atlas info
            var regex = new Regex(@"^([a-zA-Z0-9\/_]+)[\s]+([0-9]+)[\s]+([0-9]+)[\s]+([0-9]+)[\s]+([0-9]+)$");
                
            var maxX = 0u;
            var maxY = 0u;
                
            // Load atlasinfo.txt and initialize all maps the game core hosts
            if (!File.Exists("data/atlasinfo.txt"))
            {
                 throw new FileNotFoundException("Unable to find file data/atlasinfo.txt");
            }

            using var reader = new StreamReader("data/atlasinfo.txt");
            string line;
            var lineNo = 0;
            while ((line = reader.ReadLine()) != null)
            {
                lineNo++;
                if(string.IsNullOrWhiteSpace(line)) continue; // skip empty lines

                var match = regex.Match(line);
                if (match.Success)
                {
                    try
                    {
                        var mapName = match.Groups[1].Value;
                        var positionX = uint.Parse(match.Groups[2].Value);
                        var positionY = uint.Parse(match.Groups[3].Value);
                        var width = uint.Parse(match.Groups[4].Value);
                        var height = uint.Parse(match.Groups[5].Value);
                            
                        // todo check if map is hosted by this game core
                        var map = new Map(mapName, positionX, positionY, width, height);
                        _maps[map.Name] = map;

                        if (positionX + width * Map.MapUnit > maxX) maxX = positionX + width * Map.MapUnit;
                        if (positionY + height * Map.MapUnit > maxY) maxY = positionY + height * Map.MapUnit;
                    }
                    catch (FormatException)
                    {
                        throw new InvalidDataException($"Failed to parse atlasinfo.txt:line {lineNo} - Failed to parse number");
                    }
                }
                else
                {
                    throw new InvalidDataException($"Failed to parse atlasinfo.txt:line {lineNo} - Failed to parse line");
                }
            }
                
            // Initialize world grid and place maps on it
            _world.Resize(maxX / Map.MapUnit, maxY / Map.MapUnit);
            foreach (var map in _maps.Values)
            {
                for (var x = map.UnitX; x < map.UnitX + map.Width; x++)
                {
                    for (var y = map.UnitY; y < map.UnitY + map.Height; y++)
                    {
                        _world.Set(x, y, map);
                    }
                }
            }
                
            // Initialize maps, spawn monsters etc
            foreach (var map in _maps.Values)
            {
                map.Initialize();
            }
        }

        private readonly Stopwatch _sw = new Stopwatch();
        public void Update(double elapsedTime)
        {
            HookManager.Instance.CallHook<IHookWorldUpdate>(elapsedTime);

            using (_updateDuration.NewTimer())
            {
                foreach (var map in _maps.Values)
                {
                    map.Update(elapsedTime);
                }
            }
        }

        public Map GetMapAt(uint x, uint y)
        {
            var gridX = x / Map.MapUnit;
            var gridY = y / Map.MapUnit;

            return _world.Get(gridX, gridY);
        }

        public IMap GetMapByName(string name)
        {
            return _maps[name];
        }

        public List<IMap> FindMapsByName(string needle)
        {
            var list = new List<IMap>();
            foreach (var (name, map) in _maps)
            {
                if (name == needle)
                {
                    list.Clear();
                    list.Add(map);
                    return list;
                }
                
                if (name.Contains(needle, StringComparison.InvariantCultureIgnoreCase))
                {
                    list.Add(map);
                }
            }
            return list;
        }

        public SpawnGroup GetGroup(int id)
        {
            if (!_groups.ContainsKey(id))
            {
                return null;
            }
            return _groups[id];
        }

        public bool SpawnEntity(Entity e)
        {
            var map = GetMapAt((uint) e.PositionX, (uint) e.PositionY);
            if (map == null) return false;

            if (e.GetType() == typeof(PlayerEntity))
                AddPlayer((PlayerEntity)e);

            _entities.Inc();
            return map.SpawnEntity(e);
        }

        public void DespawnEntity(IEntity entity)
        {
            if (entity is PlayerEntity player)
            {
                RemovePlayer(player);
            }
            
            _entities.Dec();
            entity.Map?.DespawnEntity(entity);
        }

        public uint GenerateVid()
        {
            return ++_vid;
        }

        private void AddPlayer(PlayerEntity e)
        {
            if (_players.ContainsKey(e.Name))
                _players[e.Name] = e;
            else
                _players.Add(e.Name, e);
        }

        public void RemovePlayer(PlayerEntity e)
        {
            _players.Remove(e.Name);
        }

        public IPlayerEntity GetPlayer(string playerName)
        {
            return _players.ContainsKey(playerName) ? _players[playerName] : null;
        }
    }
}
