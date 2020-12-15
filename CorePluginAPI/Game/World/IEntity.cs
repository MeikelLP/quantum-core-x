using System;
using System.Collections.Generic;

namespace QuantumCore.API.Game.World
{
    public interface IEntity
    {
        public uint Vid { get; }
        public int PositionX { get; }
        public int PositionY { get; }
        public float Rotation { get; }
        public IMap Map { get; set; }

        public void OnDespawn();
        public void AddNearbyEntity(IEntity entity);
        public void RemoveNearbyEntity(IEntity entity);
        public void ForEachNearbyEntity(Action<IEntity> action);
        public void ShowEntity(IConnection connection);

        public void Goto(int x, int y);

        public void Move(int x, int y);
    }
}