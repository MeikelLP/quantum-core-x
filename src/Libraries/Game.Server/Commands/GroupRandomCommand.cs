using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Services;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands;

[Command("grrandom", "Spawn a monster group")]
public class GroupRandomCommand : ICommandHandler
{
    private readonly IMonsterManager _monsterManager;
    private readonly IAnimationManager _animationManager;
    private readonly IWorld _world;
    private readonly ILogger<GroupRandomCommand> _logger;
    private readonly IItemManager _itemManager;
    private readonly IDropProvider _dropProvider;
    private readonly IServiceProvider _serviceProvider;

    public GroupRandomCommand(IMonsterManager monsterManager, IAnimationManager animationManager, IWorld world,
        ILogger<GroupRandomCommand> logger, IItemManager itemManager, IDropProvider dropProvider,
        IServiceProvider serviceProvider)
    {
        _monsterManager = monsterManager;
        _animationManager = animationManager;
        _world = world;
        _logger = logger;
        _itemManager = itemManager;
        _dropProvider = dropProvider;
        _serviceProvider = serviceProvider;
    }

    public Task ExecuteAsync(CommandContext context)
    {
        var group = _world.GetRandomGroup();

        foreach (var monsterId in group.Members.Select(x => x.Id).Prepend(group.Leader))
        {
            var proto = _monsterManager.GetMonster(monsterId);
            if (proto is null)
            {
                _logger.LogWarning("Tried to spawn monster {MonsterId} in group {GroupId} but it does not exist",
                    monsterId, group.Id);
                continue;
            }

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
            var monster = new MonsterEntity(_monsterManager, _dropProvider, _animationManager, _serviceProvider, map,
                _logger, _itemManager, monsterId, x, y);
            _world.SpawnEntity(monster);
        }

        return Task.CompletedTask;
    }
}
