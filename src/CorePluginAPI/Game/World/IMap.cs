using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuantumCore.API.Game.World
{
    public interface IMap
    {
        public string Name { get; }
        public uint PositionX { get; }
        public uint UnitX { get; }
        public uint PositionY { get; }
        public uint UnitY { get; }
        public uint Width { get; }
        public uint Height { get; }

        public IReadOnlyCollection<IEntity> Entities { get; }

        public IEntity GetEntity(uint vid);

        public void SpawnEntity(IEntity entity);

        public void DespawnEntity(IEntity entity);

        public bool IsPositionInside(int x, int y);

        public void Update(double elapsedTime);
    }
}
