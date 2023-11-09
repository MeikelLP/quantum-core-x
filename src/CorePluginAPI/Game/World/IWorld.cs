using QuantumCore.API.Core.Models;

namespace QuantumCore.API.Game.World
{
    public interface IWorld
    {
        Task Load();
        void Update(double elapsedTime);
        #nullable enable
        IMap? GetMapAt(uint x, uint y);
        IMap? GetMapByName(string name);
        #nullable restore
        List<IMap> FindMapsByName(string needle);
        CoreHost GetMapHost(int x, int y);
        #nullable enable
        SpawnGroup? GetGroup(uint id);
        SpawnGroupCollection? GetGroupCollection(uint id);
        #nullable restore
        void SpawnEntity(IEntity e);
        void DespawnEntity(IEntity entity);
        uint GenerateVid();
        void RemovePlayer(IPlayerEntity e);
        IPlayerEntity? GetPlayer(string playerName);
    }
}
