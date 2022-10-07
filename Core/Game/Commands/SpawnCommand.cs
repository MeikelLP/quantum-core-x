using System.Security.Cryptography;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands
{
    [Command("spawn", "Spawn a monster or npc")]
    public class SpawnCommand : ICommandHandler<SpawnCommandOptions>
    {
        private readonly IMonsterManager _monsterManager;
        private readonly IAnimationManager _animationManager;
        private readonly IWorld _world;
        private readonly ILogger<SpawnCommand> _logger;

        public SpawnCommand(IMonsterManager monsterManager, IAnimationManager animationManager, IWorld world, ILogger<SpawnCommand> logger)
        {
            _monsterManager = monsterManager;
            _animationManager = animationManager;
            _world = world;
            _logger = logger;
        }
        
        public async Task ExecuteAsync(CommandContext<SpawnCommandOptions> context)
        {
            var proto = _monsterManager.GetMonster(context.Arguments.MonsterId);
            if (proto == null)
            {
                await context.Player.SendChatInfo("No monster found with the specified id");
            }

            for (var i = 0; i < context.Arguments.Count; i++)
            {
                // Calculate random spawn position close by the player
                var x = context.Player.PositionX + RandomNumberGenerator.GetInt32(-1500, 1501);
                var y = context.Player.PositionY + RandomNumberGenerator.GetInt32(-1500, 1501);

                // Create entity instance
                var monster = new MonsterEntity(_monsterManager, _animationManager, _world, _logger, context.Arguments.MonsterId, x, y);
                await _world.SpawnEntity(monster);
            }
        }
    }

    public class SpawnCommandOptions
    {
        [Value(0)]
        public uint MonsterId { get; set; }

        [Value(1)] public uint Count { get; set; } = 1;
    }
}