namespace QuantumCore.Game.World
{
    public abstract class Entity
    {
        public uint Vid { get; protected set; }
        public int PositionX { get; protected set; }
        public int PositionY { get; protected set; }
        public Map Map { get; set; }

        public Entity(uint vid)
        {
            Vid = vid;
        }

        public abstract void Update();
    }
}