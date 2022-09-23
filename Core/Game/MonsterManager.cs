using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QuantumCore.Core.Types;
using Serilog;

namespace QuantumCore.Game
{
    /// <summary>
    /// Manage all static data related to monster
    /// </summary>
    public class MonsterManager : IMonsterManager
    {
        private readonly ILogger<MonsterManager> _logger;
        private MobProto _proto;

        public MonsterManager(ILogger<MonsterManager> logger)
        {
            _logger = logger;
        }
        
        /// <summary>
        /// Try to load mob_proto file
        /// </summary>
        public Task LoadAsync(CancellationToken token = default)
        {
            _logger.LogInformation("Loading mob_proto");
            _proto = MobProto.FromFile("data/mob_proto");
            _logger.LogDebug("Loaded {Count} monsters", _proto.Content.Data.Monsters.Count);
            
            return Task.CompletedTask;
        }

        public MobProto.Monster GetMonster(uint id)
        {
            return _proto.Content.Data.Monsters.FirstOrDefault(monster => monster.Id == id);
        }

        public List<MobProto.Monster> GetMonsters()
        {
            return _proto.Content.Data.Monsters;
        }
    }
}