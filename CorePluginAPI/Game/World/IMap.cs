using System.Collections.Generic;

namespace QuantumCore.API.Game.World
{
    public interface IMap
    {
        public string Name { get; }
        public uint PositionX { get; }
        public uint PositionY { get; }
        public uint Width { get; }
        public uint Height { get; }

        public List<IEntity> GetEntities();

        public void DespawnEntity(IEntity entity);
    }
}