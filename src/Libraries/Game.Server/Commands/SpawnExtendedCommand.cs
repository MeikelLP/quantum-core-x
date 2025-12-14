using System.Security.Cryptography;
using CommandLine;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Services;
using QuantumCore.Game.World;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands;

[Command("mob_ld", "Spawns a monster with optional x, y and direction")]
public class SpawnExtendedCommand : ICommandHandler<SpawnExtendedCommandOptions>
{
    private readonly IMonsterManager _monsterManager;
    private readonly IWorld _world;
    private readonly IDropProvider _dropProvider;
    private readonly IAnimationManager _animationManager;
    private readonly ILogger<SpawnExtendedCommand> _logger;
    private readonly IItemManager _itemManager;
    private readonly IServiceProvider _serviceProvider;

    public SpawnExtendedCommand(IMonsterManager monsterManager, IWorld world, IDropProvider dropProvider,
        IAnimationManager animationManager, ILogger<SpawnExtendedCommand> logger, IItemManager itemManager,
        IServiceProvider serviceProvider)
    {
        _monsterManager = monsterManager;
        _world = world;
        _dropProvider = dropProvider;
        _animationManager = animationManager;
        _logger = logger;
        _itemManager = itemManager;
        _serviceProvider = serviceProvider;
    }

    public Task ExecuteAsync(CommandContext<SpawnExtendedCommandOptions> context)
    {
        var proto = _monsterManager.GetMonster(context.Arguments.MonsterId);
        if (proto is null)
        {
            context.Player.SendChatInfo("No monster found with the specified id");
            return Task.CompletedTask;
        }

        // Calculate random spawn position close by the player
        var map = context.Player.Map;
        var x = context.Arguments.PositionX is not null
            ? map.Position.X + context.Arguments.PositionX.Value * Map.SPAWN_POSITION_MULTIPLIER
            : (uint)(context.Player.PositionX + RandomNumberGenerator.GetInt32(-1500, 1501));
        var y = context.Arguments.PositionY is not null
            ? map.Position.Y + context.Arguments.PositionY.Value * Map.SPAWN_POSITION_MULTIPLIER
            : (uint)(context.Player.PositionY + RandomNumberGenerator.GetInt32(-1500, 1501));
        var rotation = context.Arguments.Rotation ?? 0;

        // Create entity instance
        var monster = new MonsterEntity(_monsterManager, _dropProvider, _animationManager, _serviceProvider, map,
            _logger, _itemManager, context.Arguments.MonsterId, (int)x, (int)y) {Rotation = rotation};
        _world.SpawnEntity(monster);

        return Task.CompletedTask;
    }
}

public class SpawnExtendedCommandOptions
{
    [Value(0, Required = true)] public uint MonsterId { get; set; }
    [Value(1)] public uint? PositionX { get; set; }
    [Value(2)] public uint? PositionY { get; set; }
    [Value(2)] public float? Rotation { get; set; }
}
