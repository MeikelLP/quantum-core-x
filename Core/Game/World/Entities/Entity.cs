using System.Collections.Generic;
using QuantumCore.Core.Utils;

namespace QuantumCore.Game.World.Entities
{
    public abstract class Entity
    {
        public uint Vid { get; protected set; }
        public int PositionX { get; protected set; }
        public int PositionY { get; protected set; }
        public Map Map { get; set; }

        public List<Entity> NearbyEntities { get; } = new List<Entity>();
        
        public const int ViewDistance = 10000;
        
        public Entity(uint vid)
        {
            Vid = vid;
        }

        protected abstract void OnNewNearbyEntity(Entity entity);
        
        public virtual void Update(double elapsedTime)
        {
            ClearNearbyEntities();
        }

        public void AddNearbyEntity(Entity entity)
        {
            NearbyEntities.Add(entity);
            OnNewNearbyEntity(entity);
        }

        private void ClearNearbyEntities()
        {
            NearbyEntities.RemoveAll(entity =>
                MathUtils.Distance(entity.PositionX, entity.PositionY, PositionX, PositionY) > ViewDistance);
        }
    }
}