﻿using CommandLine;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Services;
using QuantumCore.Game.World;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands;

[Command("mm", "Spawn a monster or npc on a random position on the current map")]
public class SpawnMapRandomCommand : ICommandHandler<SpawnMapRandomCommandOptions>
{
    private readonly IMonsterManager _monsterManager;
    private readonly IAnimationManager _animationManager;
    private readonly IWorld _world;
    private readonly ILogger<SpawnMapRandomCommand> _logger;
    private readonly IItemManager _itemManager;
    private readonly IDropProvider _dropProvider;

    public SpawnMapRandomCommand(IMonsterManager monsterManager, IAnimationManager animationManager, IWorld world,
        ILogger<SpawnMapRandomCommand> logger, IItemManager itemManager, IDropProvider dropProvider)
    {
        _monsterManager = monsterManager;
        _animationManager = animationManager;
        _world = world;
        _logger = logger;
        _itemManager = itemManager;
        _dropProvider = dropProvider;
    }

    public Task ExecuteAsync(CommandContext<SpawnMapRandomCommandOptions> context)
    {
        var proto = _monsterManager.GetMonster(context.Arguments.MonsterId);
        if (proto == null)
        {
            context.Player.SendChatInfo("No monster found with the specified id");
            return Task.CompletedTask;
        }

        var map = context.Player.Map!;
        var x = Random.Shared.Next((int)map.PositionX, (int)(map.PositionX + (map.Width * Map.MapUnit) + 1));
        var y = Random.Shared.Next((int)map.PositionY, (int)(map.PositionY + (map.Height * Map.MapUnit) + 1));

        // Create entity instance
        var monster = new MonsterEntity(_monsterManager, _dropProvider, _animationManager, map, _logger,
            _itemManager, context.Arguments.MonsterId, x, y);
        _world.SpawnEntity(monster);

        var localX = (uint)((x - map.PositionX) / (float)Map.SPAWN_POSITION_MULTIPLIER);
        var localY = (uint)((y - map.PositionY) / (float)Map.SPAWN_POSITION_MULTIPLIER);
        context.Player.SendChatInfo($"Monster spawned at ({localX}|{localY})");

        return Task.CompletedTask;
    }
}

public class SpawnMapRandomCommandOptions
{
    [Value(0, Required = true)] public uint MonsterId { get; set; }
}
