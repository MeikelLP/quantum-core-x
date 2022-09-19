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

        public List<IEntity> GetEntities();

        public IEntity GetEntity(uint vid);

        public bool SpawnEntity(IEntity entity);
        
        public Task DespawnEntity(IEntity entity);

        public bool IsPositionInside(int x, int y);

        public ValueTask Update(double elapsedTime);
    }
}