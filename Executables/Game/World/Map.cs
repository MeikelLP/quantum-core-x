using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Cache;
using QuantumCore.Core.Event;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Services;
using QuantumCore.Game.World.Entities;

// using QuantumCore.Core.API;

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

        private readonly List<IEntity> _entities = new();
        private readonly QuadTree _quadTree;
        private readonly List<SpawnPoint> _spawnPoints = new();

        private readonly List<IEntity> _nearby = new();
        private readonly List<IEntity> _remove = new();
        private readonly List<IEntity> _pendingRemovals = new();
        private readonly IMonsterManager _monsterManager;
        private readonly IAnimationManager _animationManager;
        private readonly ICacheManager _cacheManager;
        private readonly IWorld _world;
        private readonly ILogger _logger;
        private readonly ISpawnPointProvider _spawnPointProvider;
        private readonly HostingOptions _options;

        public Map(IMonsterManager monsterManager, IAnimationManager animationManager, ICacheManager cacheManager,
            IWorld world, IOptions<HostingOptions> options, ILogger logger, ISpawnPointProvider spawnPointProvider,
            string name, uint x, uint y, uint width, uint height)
        {
            _monsterManager = monsterManager;
            _animationManager = animationManager;
            _cacheManager = cacheManager;
            _world = world;
            _logger = logger;
            _spawnPointProvider = spawnPointProvider;
            _options = options.Value;
            Name = name;
            PositionX = x;
            PositionY = y;
            Width = width;
            Height = height;
            _quadTree = new QuadTree((int) x, (int) y, (int) (width * MapUnit), (int) (height * MapUnit), 20);
        }

        public async Task Initialize()
        {
            _logger.LogDebug("Load map {Name} at {PositionX}|{PositionY} (size {Width}x{Height})", Name, PositionX, PositionY, Width, Height);

            await _cacheManager.Set($"maps:{Name}", $"{IpUtils.PublicIP}:{_options.Port}");
            await _cacheManager.Publish("maps", $"{Name} {IpUtils.PublicIP}:{_options.Port}");

            _spawnPoints.AddRange(await _spawnPointProvider.GetSpawnPointsForMap(Name));

            _logger.LogDebug("Loaded {SpawnPointsCount} spawn points for map {MapName}", _spawnPoints.Count, Name);

            // Populate map
            foreach(var spawnPoint in _spawnPoints)
            {
                var monsterGroup = new MonsterGroup { SpawnPoint = spawnPoint };
                await SpawnGroup(monsterGroup);
            }
        }

        public async ValueTask Update(double elapsedTime)
        {
            // HookManager.Instance.CallHook<IHookMapUpdate>(this, elapsedTime);

            foreach (var entity in _pendingRemovals)
            {
                _entities.Remove(entity as Entity);
            }
            _pendingRemovals.Clear();

            foreach (var entity in _entities.ToArray())
            {
                await entity.Update(elapsedTime);

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
                        await entity.ForEachNearbyEntity(e =>
                        {
                            // Remove this entity from our temporary list as they are already in it
                            if (!_nearby.Remove(e))
                            {
                                // If it wasn't in our temporary list it is no longer in view
                                _remove.Add(e);
                            }

                            return Task.CompletedTask;
                        });

                        // Remove previously marked entities on both sides
                        foreach (var e in _remove)
                        {
                            await e.RemoveNearbyEntity(entity);
                            await entity.RemoveNearbyEntity(e);
                        }

                        // Add new nearby entities on both sides
                        foreach (var e in _nearby)
                        {
                            if (e == entity)
                            {
                                continue; // do not add ourself!
                            }

                            await e.AddNearbyEntity(entity);
                            await entity.AddNearbyEntity(e);
                        }

                        // Clear our temporary lists
                        _nearby.Clear();
                        _remove.Clear();
                    }
                }
            }
        }

        private async Task SpawnGroup(MonsterGroup groupInstance)
        {
            var spawnPoint = groupInstance.SpawnPoint;
            switch (spawnPoint.Type)
            {
                case ESpawnPointType.GroupCollection:
                    var groupCollection = _world.GetGroupCollection(spawnPoint.Monster);
                    if (groupCollection != null)
                    {
                        foreach (var member in groupCollection.Members)
                        {
                            var group = _world.GetGroup(member.Id);
                            if (group != null)
                            {
                                for (int i = 0; i < member.Amount; i++)
                                {
                                    await SpawnGroup(groupInstance, spawnPoint, group);
                                }
                            }
                        }
                    }
                    break;
                case ESpawnPointType.Group:
                {
                    var group = _world.GetGroup(spawnPoint.Monster);
                    if (group != null)
                    {
                        await SpawnGroup(groupInstance, spawnPoint, group);
                    }

                    break;
                }
                case ESpawnPointType.Monster:
                {
                    var x = (int)PositionX + (spawnPoint.X + CoreRandom.GenerateInt32(-spawnPoint.RangeX, spawnPoint.RangeY + 1)) * 100;
                    var y = (int)PositionY + (spawnPoint.Y + CoreRandom.GenerateInt32(-spawnPoint.RangeX, spawnPoint.RangeY + 1)) * 100;

                    spawnPoint.CurrentGroup = groupInstance;

                    var monster = new MonsterEntity(_monsterManager, _animationManager, _world, _logger, spawnPoint.Monster, x, y, (spawnPoint.Direction - 1) * 45);
                    await _world.SpawnEntity(monster);

                    groupInstance.Monsters.Add(monster);
                    monster.Group = groupInstance;

                    break;
                }
                default:
                    _logger.LogWarning("Unknown spawn point type: {SpawnPointType}", spawnPoint.Type);
                    break;
            }
        }

        private async Task SpawnGroup(MonsterGroup groupInstance, SpawnPoint spawnPoint, SpawnGroup group)
        {
            var baseX = spawnPoint.X + RandomNumberGenerator.GetInt32(-spawnPoint.RangeX, spawnPoint.RangeY);
            var baseY = spawnPoint.Y + RandomNumberGenerator.GetInt32(-spawnPoint.RangeX, spawnPoint.RangeY);

            spawnPoint.CurrentGroup = groupInstance;

            foreach (var member in group.Members)
            {
                var monster = new MonsterEntity(_monsterManager, _animationManager, _world, _logger, member.Id,
                    (int) (PositionX + (baseX + RandomNumberGenerator.GetInt32(-5, 5)) * 100),
                    (int) (PositionY + (baseY + RandomNumberGenerator.GetInt32(-5, 5)) * 100),
                    RandomNumberGenerator.GetInt32(0, 360));
                await _world.SpawnEntity(monster);

                groupInstance.Monsters.Add(monster);
                monster.Group = groupInstance;
            }
        }

        public void EnqueueGroupRespawn(MonsterGroup group)
        {
            EventSystem.EnqueueEvent(() =>
            {
                // TODO
                SpawnGroup(group).Wait();
                return 0;
            }, group.SpawnPoint.RespawnTime * 1000);
        }

        public bool IsPositionInside(int x, int y)
        {
            return x >= PositionX && x < PositionX + Width * MapUnit && y >= PositionY && y < PositionY + Height * MapUnit;
        }

        public bool SpawnEntity(IEntity entity)
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
        /// Add a ground item which will automatically get destroyed after configured time
        /// </summary>
        /// <param name="item">Item to add on the ground, should not have any owner!</param>
        /// <param name="x">Position X</param>
        /// <param name="y">Position Y</param>
        /// <param name="amount">Only used for gold as we have a higher limit here</param>
        public void AddGroundItem(ItemInstance item, int x, int y, uint amount = 0)
        {
            var groundItem = new GroundItem(_animationManager, _world.GenerateVid(), item, amount) {
                PositionX = x,
                PositionY = y
            };

            SpawnEntity(groundItem);
        }

        /// <summary>
        /// Should only be called by World
        /// </summary>
        /// <param name="entity"></param>
        public async Task DespawnEntity(IEntity entity)
        {
            _logger.LogDebug("Despawn {Entity}", entity);

            // Call despawn handlers
            await entity.OnDespawn();

            // Remove this entity from all nearby entities
            await entity.ForEachNearbyEntity(async e => await e.RemoveNearbyEntity(entity));

            // Remove map from the entity
            entity.Map = null;

            lock (_entities)
            {
                _entities.Remove(entity);
            }
            // Remove entity from the quad tree
            _quadTree.Remove(entity);

            // Remove entity from entities list in the next update
            _pendingRemovals.Add(entity);
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
