using System.Collections.Generic;
using System.Linq;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Core.API;
using QuantumCore.Core.Utils;
using QuantumCore.Game.World.Entities;
using Serilog;

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

        private readonly List<Entity> _entities;
        private readonly QuadTree<Entity> _quadTree;

        public Map(string name, uint x, uint y, uint width, uint height)
        {
            Name = name;
            PositionX = x;
            PositionY = y;
            Width = width;
            Height = height;
            _entities = new List<Entity>();
            _quadTree = new QuadTree<Entity>((int) x, (int) y, (int) (width * MapUnit), (int) (height * MapUnit), 20);
        }

        public void Initialize()
        {
            Log.Debug($"Load map '{Name}' at {PositionX}x{PositionY} (size {Width}x{Height})");
        }

        public void Update(double elapsedTime)
        {
            HookManager.Instance.CallHook<IHookMapUpdate>(this, elapsedTime);
            
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
        
        public bool SpawnEntity(Entity entity)
        {
            if (!_quadTree.Insert(entity)) return false;
            
            // Add this entity to all entities nearby
            var nearby = new List<Entity>();
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

        public void DespawnEntity(Entity entity)
        {
            Log.Debug($"Despawn {entity}");
            
            // Remove this entity from all nearby entities
            foreach (var e in entity.NearbyEntities)
            {
                e.RemoveNearbyEntity(entity);
            }

            // Remove map from the entity
            entity.Map = null;
            
            // Remove entity from the quad tree
            _quadTree.Remove(entity);
            
            // Call despawn handlers
            entity.OnDespawn();
        }

        public List<IEntity> GetEntities()
        {
            return new List<IEntity>(_entities);
        }
    }
}