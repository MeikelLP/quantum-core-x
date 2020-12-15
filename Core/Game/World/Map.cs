using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
    public class Map : IMap
    {
        public const uint MapUnit = 25600;
        public string Name { get; private set; }
        public uint PositionX { get; private set; }
        public uint UnitX => PositionX / MapUnit;
        public uint PositionY { get; private set; }
        public uint UnitY => PositionY / MapUnit;
        public uint Width { get; private set; }
        public uint Height { get; private set; }

        private readonly List<Entity> _entities = new List<Entity>();
        private readonly QuadTree<IEntity> _quadTree;
        private readonly List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();

        public Map(string name, uint x, uint y, uint width, uint height)
        {
            Name = name;
            PositionX = x;
            PositionY = y;
            Width = width;
            Height = height;
            _quadTree = new QuadTree<IEntity>((int) x, (int) y, (int) (width * MapUnit), (int) (height * MapUnit), 20);
        }

        public void Initialize()
        {
            Log.Debug($"Load map '{Name}' at {PositionX}x{PositionY} (size {Width}x{Height})");

            // Load map spawn data
            var spawnFile = Path.Join("data", "maps", Name, "spawn.toml");
            if (File.Exists(spawnFile))
            {
                var spawns = Toml.Parse(File.ReadAllText(spawnFile));
                var model = spawns.ToModel();
                var spawnPoints = model["spawn"] as TomlTableArray;
                if (spawnPoints == null)
                {
                    // No spawn points defined
                    return;
                }
                
                foreach (var point in spawnPoints)
                {
                    // Construct/parse spawn point
                    _spawnPoints.Add(SpawnPoint.FromToml(point));
                }
                
                Log.Debug($"Loaded {_spawnPoints.Count} spawn points");
            }
            
            // Populate map
            foreach(var spawnPoint in _spawnPoints) {
                switch (spawnPoint.Type)
                {
                    case ESpawnPointType.Group:
                        var group = World.Instance.GetGroup(CoreRandom.GetRandom(spawnPoint.Groups));
                        if (group != null)
                        {
                            var baseX = spawnPoint.X + RandomNumberGenerator.GetInt32(-spawnPoint.Range, spawnPoint.Range);
                            var baseY = spawnPoint.Y + RandomNumberGenerator.GetInt32(-spawnPoint.Range, spawnPoint.Range);
                            
                            foreach (var member in group.Members)
                            {
                                var monster = new MonsterEntity(member.Id,
                                    (int) (PositionX + (baseX + RandomNumberGenerator.GetInt32(-5, 5)) * 100),
                                    (int) (PositionY + (baseY + RandomNumberGenerator.GetInt32(-5, 5)) * 100));
                                World.Instance.SpawnEntity(monster);
                            }
                        }
                        break;
                }
            }
        }

        public void Update(double elapsedTime)
        {
            HookManager.Instance.CallHook<IHookMapUpdate>(this, elapsedTime);

            lock (_entities)
            {
                foreach (var entity in _entities)
                {
                    entity.Update(elapsedTime);

                    if (entity.PositionChanged)
                    {
                        _quadTree.Remove(entity);
                        _quadTree.Insert(entity);
                        entity.PositionChanged = false;

                        // todo Refresh nearby entities
                    }
                }
            }
        }
        
        public bool SpawnEntity(Entity entity)
        {
            lock (_entities)
            {
                if (!_quadTree.Insert(entity)) return false;

                // Add this entity to all entities nearby
                var nearby = new List<IEntity>();
                _quadTree.QueryAround(nearby, entity.PositionX, entity.PositionY, Entity.ViewDistance);
                foreach (var e in nearby.Where(e => e != entity))
                {
                    entity.AddNearbyEntity(e);
                    e.AddNearbyEntity(entity);
                }

                _entities.Add(entity);
                entity.Map = this;
                return true;
            }
        }

        public void DespawnEntity(IEntity entity)
        {
            lock (_entities)
            {
                Log.Debug($"Despawn {entity}");

                // Remove this entity from all nearby entities
                entity.ForEachNearbyEntity(e => e.RemoveNearbyEntity(entity));

                // Remove map from the entity
                entity.Map = null;

                // Remove entity from the quad tree
                _quadTree.Remove(entity);

                // Call despawn handlers
                entity.OnDespawn();
            }
        }

        public List<IEntity> GetEntities()
        {
            // todo make sure if we have to lock here or not
            lock (_entities)
            {
                return new List<IEntity>(_entities);
            }
        }
    }
}