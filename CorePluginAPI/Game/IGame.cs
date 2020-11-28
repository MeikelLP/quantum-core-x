using QuantumCore.API.Game.World;

namespace QuantumCore.API.Game
{
    public interface IGame
    {
        public IWorld World { get; }
    }
}