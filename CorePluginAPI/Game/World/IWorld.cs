using System.Collections.Generic;
using System.Threading.Tasks;
using QuantumCore.API.Core.Models;

namespace QuantumCore.API.Game.World
{
    public interface IWorld
    {
        static IWorld Instance { get; set; }
        Task Load();
        void Update(double elapsedTime);
        IMap GetMapAt(uint x, uint y);
        IMap GetMapByName(string name);
        List<IMap> FindMapsByName(string needle);
        CoreHost GetMapHost(int x, int y);
        SpawnGroup GetGroup(int id);
        ValueTask<bool> SpawnEntity(IEntity e);
        Task DespawnEntity(IEntity entity);
        uint GenerateVid();
        void RemovePlayer(IPlayerEntity e);
        IPlayerEntity GetPlayer(string playerName);
    }
}