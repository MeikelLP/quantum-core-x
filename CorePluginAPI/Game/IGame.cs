using QuantumCore.API.Game.World;
using System;

namespace QuantumCore.API.Game
{
    public interface IGame
    {
        public IWorld World { get; }

        public void RegisterCommandNamespace(Type t);
    }
}