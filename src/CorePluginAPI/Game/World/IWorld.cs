using QuantumCore.API.Core.Models;

namespace QuantumCore.API.Game.World
{
    public interface IWorld : ILoadable
    {
        Task InitAsync();
        void Update(double elapsedTime);
#nullable enable
        IMap? GetMapAt(uint x, uint y);
        IMap? GetMapByName(string name);
#nullable restore
        List<IMap> FindMapsByName(string needle);
        CoreHost GetMapHost(int x, int y);
#nullable enable
        SpawnGroup? GetGroup(uint id);
        SpawnGroup GetRandomGroup();
        SpawnGroupCollection? GetGroupCollection(uint id);
#nullable restore
        void SpawnEntity(IEntity e);

        /// <summary>
        /// This will immediately despawn an entity. However, it does not trigger persistence for players. Please use
        /// <see cref="DespawnPlayerAsync"/> for this.
        /// </summary>
        /// <param name="entity"></param>
        void DespawnEntity(IEntity entity);

        /// <summary>
        /// Despawns a player and waits for persistence to be finished before returning
        /// </summary>
        Task DespawnPlayerAsync(IPlayerEntity player);

        uint GenerateVid();
        void RemovePlayer(IPlayerEntity e);
        IPlayerEntity? GetPlayer(string playerName);
        IList<IPlayerEntity> GetPlayers();
        IPlayerEntity? GetPlayerById(uint playerId);
    }
}
