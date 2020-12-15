using System;
using System.Collections.Generic;
using QuantumCore.API;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Networking;
using QuantumCore.Core.Utils;

namespace QuantumCore.Game.World.Entities
{
    public abstract class Entity : IEntity
    {
        public uint Vid { get; protected set; }
        public int PositionX
        {
            get => _positionX;
            set {
                _positionChanged = _positionChanged || _positionX != value;
                _positionX = value;
            }
        }
        public int PositionY
        {
            get => _positionY;
            set {
                _positionChanged = _positionChanged || _positionY != value;
                _positionY = value;
            }
        }
        public float Rotation
        {
            get => _rotation;
            set => _rotation = value;
        }

        public bool PositionChanged
        {
            get => _positionChanged;
            set => _positionChanged = value;
        }

        public IMap Map { get; set; }
        private List<IEntity> NearbyEntities { get; } = new List<IEntity>();
        public const int ViewDistance = 10000;

        private int _positionX;
        private int _positionY;
        private float _rotation;
        private bool _positionChanged;
        
        public Entity(uint vid)
        {
            Vid = vid;
        }

        protected abstract void OnNewNearbyEntity(IEntity entity);
        protected abstract void OnRemoveNearbyEntity(IEntity entity);
        public abstract void OnDespawn();

        public abstract void ShowEntity(IConnection connection);
        
        public virtual void Update(double elapsedTime)
        {
            ClearNearbyEntities();
        }

        public void AddNearbyEntity(IEntity entity)
        {
            lock (NearbyEntities)
            {
                NearbyEntities.Add(entity);
                OnNewNearbyEntity(entity);
            }
        }

        public void RemoveNearbyEntity(IEntity entity)
        {
            lock (NearbyEntities)
            {
                if (NearbyEntities.Remove(entity))
                {
                    OnRemoveNearbyEntity(entity);
                }
            }
        }

        private void ClearNearbyEntities()
        {
            lock (NearbyEntities)
            {
                NearbyEntities.RemoveAll(entity =>
                    MathUtils.Distance(entity.PositionX, entity.PositionY, PositionX, PositionY) > ViewDistance);
            }
        }

        public void ForEachNearbyEntity(Action<IEntity> action)
        {
            lock (NearbyEntities)
            {
                foreach (var entity in NearbyEntities)
                {
                    action(entity);
                }   
            }
        }
    }
}