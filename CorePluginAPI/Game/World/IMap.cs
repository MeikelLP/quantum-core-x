using QuantumCore.API.Core.Models;

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
        public IWorld World { get; }

        public IReadOnlyCollection<IEntity> Entities { get; }

        public IEntity? GetEntity(uint vid);

        public void SpawnEntity(IEntity entity);

        public void DespawnEntity(IEntity entity);

        public bool IsPositionInside(int x, int y);

        public void Update(double elapsedTime);

        /// <summary>
        /// Add a ground item which will automatically get destroyed after configured time
        /// </summary>
        /// <param name="item">Item to add on the ground, should not have any owner!</param>
        /// <param name="x">Position X</param>
        /// <param name="y">Position Y</param>
        /// <param name="amount">Only used for gold as we have a higher limit here</param>
        /// <param name="ownerName"></param>
        void AddGroundItem(ItemInstance item, int x, int y, uint amount = 0, string? ownerName = null);
    }
}
