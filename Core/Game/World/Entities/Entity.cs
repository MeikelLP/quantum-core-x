using System;
using System.Collections.Generic;
using System.Diagnostics;
using QuantumCore.API;
using QuantumCore.API.Core.Utils;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Networking;
using QuantumCore.Core.Utils;
using Serilog;

namespace QuantumCore.Game.World.Entities
{
    public abstract class Entity : IEntity
    {
        public uint Vid { get; }
        public abstract EEntityType Type { get; }
        public uint EntityClass { get; protected set; }
        public EEntityState State { get; protected set; }
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

        public long Health { get; set; }
        public abstract byte HealthPercentage { get; }
        public bool Dead { get; private set; }
        
        public IMap Map { get; set; }
        
        // QuadTree cache
        public int LastPositionX { get; set; }
        public int LastPositionY { get; set; }
        public IQuadTree LastQuadTree { get; set; }
        
        // Movement related
        public long MovementStart { get; private set; }
        public int TargetPositionX { get; private set; }
        public int StartPositionX { get; private set; }
        public int TargetPositionY { get; private set; }
        public int StartPositionY { get; private set; }
        public uint MovementDuration { get; private set; }
        public byte MovementSpeed { get; protected set; }

        private List<IEntity> NearbyEntities { get; } = new();
        public List<IPlayerEntity> TargetedBy { get; } = new();
        public const int ViewDistance = 10000;

        private int _positionX;
        private int _positionY;
        private float _rotation;
        private bool _positionChanged;
        private IEntity _entityImplementation;

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
            if (State == EEntityState.Moving)
            {
                var elapsed = GameServer.Instance.Server.ServerTime - MovementStart;
                var rate = MovementDuration == 0 ? 1 : elapsed / (float) MovementDuration;
                if (rate > 1) rate = 1;

                var x = (int)((TargetPositionX - StartPositionX) * rate + StartPositionX);
                var y = (int)((TargetPositionY - StartPositionY) * rate + StartPositionY);

                PositionX = x;
                PositionY = y;

                if (rate >= 1)
                {
                    State = EEntityState.Idle;
                }
            }
        }
        
        public virtual void Move(int x, int y)
        {
            if (PositionX == x && PositionY == y) return;
            PositionX = x;
            PositionY = y;
            PositionChanged = true;
        }

        public virtual void Goto(int x, int y)
        {
            if (PositionX == x && PositionY == y) return;
            if (TargetPositionX == x && TargetPositionY == y) return;

            var animation =
                AnimationManager.GetAnimation(EntityClass, AnimationType.Run, AnimationSubType.General);

            State = EEntityState.Moving;
            TargetPositionX = x;
            TargetPositionY = y;
            StartPositionX = PositionX;
            StartPositionY = PositionY;
            MovementStart = GameServer.Instance.Server.ServerTime;

            var distance = MathUtils.Distance(StartPositionX, StartPositionY, TargetPositionX, TargetPositionY);
            if (animation == null)
            {
                MovementDuration = 0;
            }
            else
            {
                var animationSpeed = -animation.AccumulationY / animation.MotionDuration;
                var i = 100 - MovementSpeed;
                if (i > 0)
                {
                    i = 100 + i;
                } else if (i < 0)
                {
                    i = 10000 / (100 - i);
                }
                else
                {
                    i = 100;
                }

                var duration = (int) ((distance / animationSpeed) * 1000) * i / 100;
                MovementDuration = (uint) duration;
            }
        }

        public void Stop()
        {
            State = EEntityState.Idle;
            MovementDuration = 0;
        }

        public virtual void Die()
        {
            Dead = true;
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