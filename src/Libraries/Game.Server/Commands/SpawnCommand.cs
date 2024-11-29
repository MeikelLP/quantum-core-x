using System.Security.Cryptography;
using CommandLine;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Services;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands;

[Command("m", "Spawn a monster or npc")]
[Command("mob", "Spawn a monster or npc")]
[Command("spawn", "Spawn a monster or npc")]
public class SpawnCommand : ICommandHandler<SpawnCommandOptions>
{
    private readonly IMonsterManager _monsterManager;
    private readonly IAnimationManager _animationManager;
    private readonly IWorld _world;
    private readonly ILogger<SpawnCommand> _logger;
    private readonly IItemManager _itemManager;
    private readonly IDropProvider _dropProvider;

    public SpawnCommand(IMonsterManager monsterManager, IAnimationManager animationManager, IWorld world,
        ILogger<SpawnCommand> logger, IItemManager itemManager, IDropProvider dropProvider)
    {
        _monsterManager = monsterManager;
        _animationManager = animationManager;
        _world = world;
        _logger = logger;
        _itemManager = itemManager;
        _dropProvider = dropProvider;
    }

    public Task ExecuteAsync(CommandContext<SpawnCommandOptions> context)
    {
        var proto = _monsterManager.GetMonster(context.Arguments.MonsterId);
        if (proto == null)
        {
            context.Player.SendChatInfo("No monster found with the specified id");
            return Task.CompletedTask;
        }

        for (var i = 0; i < context.Arguments.Count; i++)
        {
            // Calculate random spawn position close by the player
            var x = context.Player.PositionX + RandomNumberGenerator.GetInt32(-1500, 1501);
            var y = context.Player.PositionY + RandomNumberGenerator.GetInt32(-1500, 1501);

            var map = _world.GetMapAt((uint)x, (uint)y);

            if (map is null)
            {
                context.Player.SendChatInfo("Map could not be found. This shouldn't happen");
                return Task.CompletedTask;
            }

            // Create entity instance
            var monster = new MonsterEntity(_monsterManager, _dropProvider, _animationManager, map, _logger,
                _itemManager, context.Arguments.MonsterId, x, y);
            _world.SpawnEntity(monster);
        }

        return Task.CompletedTask;
    }
}

public class SpawnCommandOptions
{
    [Value(0, Required = true)] public uint MonsterId { get; set; }

    [Value(1)] public uint Count { get; set; } = 1;
}
