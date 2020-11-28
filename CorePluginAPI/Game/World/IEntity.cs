namespace QuantumCore.API.Game.World
{
    public interface IEntity
    {
        public uint Vid { get; }
        public int PositionX { get; }
        public int PositionY { get; }
        public float Rotation { get; }
    }
}