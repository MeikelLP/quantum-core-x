using System;
using System.Collections.Generic;
using QuantumCore.API.Core.Utils;

namespace QuantumCore.API.Game.World
{
    public enum EEntityState
    {
        Idle,
        Moving
    }

    public enum EEntityType
    {
        Monster = 0,
        Npc = 1,
        Player = 6
    }
    
    public interface IEntity
    {
        public uint Vid { get; }
        public EEntityType Type { get; }
        public EEntityState State { get; }
        public int PositionX { get; }
        public int PositionY { get; }
        public float Rotation { get; }
        public IMap Map { get; set; }
        public byte HealthPercentage { get; }
        
        // QuadTree cache
        public int LastPositionX { get; set; }
        public int LastPositionY { get; set; }
        public IQuadTree LastQuadTree { get; set; }

        // Movement related
        public long MovementStart { get; }
        public int TargetPositionX { get; }
        public int StartPositionX { get; }
        public int TargetPositionY { get; }
        public int StartPositionY { get; }
        public uint MovementDuration { get; }
        
        public void OnDespawn();
        public void AddNearbyEntity(IEntity entity);
        public void RemoveNearbyEntity(IEntity entity);
        public void ForEachNearbyEntity(Action<IEntity> action);
        public void ShowEntity(IConnection connection);

        public void Goto(int x, int y);

        public void Move(int x, int y);
        public void Stop();
    }
}