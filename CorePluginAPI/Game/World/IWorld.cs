namespace QuantumCore.API.Game.World
{
    public interface IWorld
    {
        public IMap GetMapByName(string name);
    }
}