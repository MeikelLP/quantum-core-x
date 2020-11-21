namespace QuantumCore.Game.World
{
    public abstract class Entity
    {
        public uint Vid { get; protected set; }
        public int PositionX { get; protected set; }
        public int PositionY { get; protected set; }

        public Entity(uint vid)
        {
            Vid = vid;
        }
    }
}