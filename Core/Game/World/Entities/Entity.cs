using System.Collections.Generic;
using QuantumCore.Core.Networking;
using QuantumCore.Core.Utils;

namespace QuantumCore.Game.World.Entities
{
    public abstract class Entity
    {
        public uint Vid { get; protected set; }
        public int PositionX
        {
            get => _positionX;
            set { 
                _positionX = value;
                _positionChanged = true;
            }
        }
        public int PositionY
        {
            get => _positionY;
            set { 
                _positionY = value;
                _positionChanged = true;
            }
        }

        public float Rotation
        {
            get => _rotation;
            set => _rotation = value;
        }

        public Map Map { get; set; }
        public List<Entity> NearbyEntities { get; } = new List<Entity>();
        public const int ViewDistance = 10000;

        private int _positionX;
        private int _positionY;
        private float _rotation;
        private bool _positionChanged;
        
        public Entity(uint vid)
        {
            Vid = vid;
        }

        protected abstract void OnNewNearbyEntity(Entity entity);
        protected abstract void OnRemoveNearbyEntity(Entity entity);

        public abstract void ShowEntity(Connection connection);
        
        public virtual void Update(double elapsedTime)
        {
            ClearNearbyEntities();
            
            // todo refresh quad tree position
        }

        public void AddNearbyEntity(Entity entity)
        {
            NearbyEntities.Add(entity);
            OnNewNearbyEntity(entity);
        }

        public void RemoveNearbyEntity(Entity entity)
        {
            if (NearbyEntities.Remove(entity))
            {
                OnRemoveNearbyEntity(entity);
            }
        }

        private void ClearNearbyEntities()
        {
            NearbyEntities.RemoveAll(entity =>
                MathUtils.Distance(entity.PositionX, entity.PositionY, PositionX, PositionY) > ViewDistance);
        }
    }
}