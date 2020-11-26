namespace QuantumCore.API.Game
{
    public interface IWorld
    {
        public IMap GetMapByName(string name);
    }
}