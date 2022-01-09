using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Core.API;
using QuantumCore.Core.Event;
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

        private readonly List<Entity> _entities = new();
        private readonly QuadTree _quadTree;
        private readonly List<SpawnPoint> _spawnPoints = new();

        private readonly List<IEntity> _nearby = new();
        private readonly List<IEntity> _remove = new();
        private readonly List<IEntity> _pendingRemovals = new();
        
        public Map(string name, uint x, uint y, uint width, uint height)
        {
            Name = name;
            PositionX = x;
            PositionY = y;
            Width = width;
            Height = height;
            _quadTree = new QuadTree((int) x, (int) y, (int) (width * MapUnit), (int) (height * MapUnit), 20);
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
            foreach(var spawnPoint in _spawnPoints) 
            {
                var monsterGroup = new MonsterGroup { SpawnPoint = spawnPoint };
                SpawnGroup(monsterGroup);
            }
        }

        public void Update(double elapsedTime)
        {
            HookManager.Instance.CallHook<IHookMapUpdate>(this, elapsedTime);

            lock (_entities)
            {
                foreach (var entity in _pendingRemovals)
                {
                    _entities.Remove(entity as Entity);
                }
                _pendingRemovals.Clear();
                
                foreach (var entity in _entities)
                {
                    entity.Update(elapsedTime);

                    if (entity.PositionChanged)
                    {
                        entity.PositionChanged = false;
                        
                        // Update position in our quad tree (used for faster nearby look up)
                        _quadTree.UpdatePosition(entity);

                        if (entity.Type == EEntityType.Player)
                        {
                            // Check which entities are relevant for nearby
                            EEntityType? filter = null;
                            if (entity.Type != EEntityType.Player)
                            {
                                // if we aren't a player only players are relevant for nearby
                                filter = EEntityType.Player;
                            }

                            // Update entities nearby
                            _quadTree.QueryAround(_nearby, entity.PositionX, entity.PositionY, Entity.ViewDistance,
                                filter);

                            // Check nearby entities and mark all entities which are too far away now
                            entity.ForEachNearbyEntity(e =>
                            {
                                // Remove this entity from our temporary list as they are already in it
                                if (!_nearby.Remove(e))
                                {
                                    // If it wasn't in our temporary list it is no longer in view
                                    _remove.Add(e);
                                }
                            });

                            // Remove previously marked entities on both sides
                            foreach (var e in _remove)
                            {
                                e.RemoveNearbyEntity(entity);
                                entity.RemoveNearbyEntity(e);
                            }

                            // Add new nearby entities on both sides
                            foreach (var e in _nearby)
                            {
                                if (e == entity)
                                {
                                    continue; // do not add ourself!
                                }

                                e.AddNearbyEntity(entity);
                                entity.AddNearbyEntity(e);
                            }

                            // Clear our temporary lists
                            _nearby.Clear();
                            _remove.Clear();
                        }
                    }
                }
            }
        }

        private void SpawnGroup(MonsterGroup groupInstance)
        {
            var spawnPoint = groupInstance.SpawnPoint;
            switch (spawnPoint.Type)
            {
                case ESpawnPointType.Group:
                    var group = World.Instance.GetGroup(CoreRandom.GetRandom(spawnPoint.Groups));
                    if (group != null)
                    {
                        var baseX = spawnPoint.X + RandomNumberGenerator.GetInt32(-spawnPoint.Range, spawnPoint.Range);
                        var baseY = spawnPoint.Y + RandomNumberGenerator.GetInt32(-spawnPoint.Range, spawnPoint.Range);
                        
                        spawnPoint.CurrentGroup = groupInstance;
                            
                        foreach (var member in group.Members)
                        {
                            var monster = new MonsterEntity(member.Id,
                                (int) (PositionX + (baseX + RandomNumberGenerator.GetInt32(-5, 5)) * 100),
                                (int) (PositionY + (baseY + RandomNumberGenerator.GetInt32(-5, 5)) * 100),
                                RandomNumberGenerator.GetInt32(0, 360));
                            World.Instance.SpawnEntity(monster);
                                
                            groupInstance.Monsters.Add(monster);
                            monster.Group = groupInstance;
                        }
                    }
                    break;
                default:
                    Log.Warning($"Unknown spawn point type: {spawnPoint.Type}");
                    break;
            }
        }

        public void EnqueueGroupRespawn(MonsterGroup group)
        {
            EventSystem.EnqueueEvent(() =>
            {
                SpawnGroup(group);
                return 0;
            }, group.SpawnPoint.RespawnTime * 1000);
        }
        
        public bool SpawnEntity(Entity entity)
        {
            lock (_entities)
            {
                if (!_quadTree.Insert(entity)) return false;

                // Add this entity to all entities nearby
                var nearby = new List<IEntity>();
                EEntityType? filter = null;
                if (entity.Type != EEntityType.Player)
                {
                    // if we aren't a player only players are relevant for nearby
                    filter = EEntityType.Player;
                }
                
                _quadTree.QueryAround(nearby, entity.PositionX, entity.PositionY, Entity.ViewDistance, filter);
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

        /// <summary>
        /// Should only be called by World
        /// </summary>
        /// <param name="entity"></param>
        public void DespawnEntity(IEntity entity)
        {
            lock (_entities)
            {
                Log.Debug($"Despawn {entity}");

                // Call despawn handlers
                entity.OnDespawn();

                // Remove this entity from all nearby entities
                entity.ForEachNearbyEntity(e => e.RemoveNearbyEntity(entity));

                // Remove map from the entity
                entity.Map = null;

                // Remove entity from the quad tree
                _quadTree.Remove(entity);

                // Remove entity from entities list in the next update
                _pendingRemovals.Add(entity);
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

        public IEntity GetEntity(uint vid)
        {
            lock (_entities)
            {
                return _entities.Find(e => e.Vid == vid);
            }
        }
    }
}