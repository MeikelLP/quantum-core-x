namespace QuantumCore.API.Game
{
    public interface IEntity
    {
        public uint Vid { get; }
        public int PositionX { get; }
        public int PositionY { get; }
        public float Rotation { get; }
    }
}