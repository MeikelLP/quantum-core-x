using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.Core.Types;

namespace QuantumCore.Game
{
    /// <summary>
    /// Manage all static data related to monster
    /// </summary>
    public class MonsterManager : IMonsterManager
    {
        private readonly ILogger<MonsterManager> _logger;
        private ImmutableArray<MonsterData> _proto = [];

        public MonsterManager(ILogger<MonsterManager> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Try to load mob_proto file
        /// </summary>
        public async Task LoadAsync(CancellationToken token = default)
        {
            _logger.LogInformation("Loading mob_proto");
            _proto = [..await new MobProtoLoader().LoadAsync("data/mob_proto")];
            _logger.LogDebug("Loaded {Count} monsters", _proto.Length);
        }

        public MonsterData? GetMonster(uint id)
        {
            return _proto.FirstOrDefault(monster => monster.Id == id);
        }

        public ImmutableArray<MonsterData> GetMonsters()
        {
            return _proto;
        }
    }
}
