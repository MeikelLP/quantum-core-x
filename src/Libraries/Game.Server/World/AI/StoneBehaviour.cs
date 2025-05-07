using System.Collections.Immutable;
using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.World;
using QuantumCore.API.Game.World.AI;
using QuantumCore.Game.Services;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.World.AI;

public class StoneBehaviour : IBehaviour
{
    private readonly IWorld _world;
    private IEntity _entity;
    private readonly IMonsterManager _monsterManager;
    private readonly IDropProvider _dropProvider;
    private readonly IAnimationManager _animationManager;
    private readonly IItemManager _itemManager;
    private readonly ILogger<StoneBehaviour> _logger;
    private readonly IServiceProvider _serviceProvider;
    private ImmutableArray<SpawnGroup> _groups;
    private byte HealthChunkCount = 10;
    private long _lastChunk;
    private uint _chunkSize;
    private readonly List<IEntity> _spawnedEntities = new();

    public StoneBehaviour(IWorld world, IMonsterManager monsterManager, IDropProvider dropProvider,
        IAnimationManager animationManager, IItemManager itemManager, ILogger<StoneBehaviour> logger,
        IServiceProvider serviceProvider)
    {
        _world = world;
        _monsterManager = monsterManager;
        _dropProvider = dropProvider;
        _animationManager = animationManager;
        _itemManager = itemManager;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public void Init(IEntity entity)
    {
        if (entity is not MonsterEntity mob)
        {
            Debug.Assert(entity is MonsterEntity, "Stone behaviours can only be applied to monster entities");
            return;
        }

        _entity = entity;

        var groupsFrom = mob.Proto.AttackSpeed;
        var groupsTo = mob.Proto.MoveSpeed;
        _groups =
        [
            ..Enumerable.Range(groupsFrom, groupsTo - groupsFrom)
                .Select(x => _world.GetGroup((uint)x))
                .Where(x => x is not null)
                .Cast<SpawnGroup>()
        ];
        if (_groups.IsEmpty)
        {
            _logger.LogWarning("Stone has no groups. You are probably missing the group.txt or group_group.txt");
        }

        _chunkSize = (uint)(mob.Proto.Hp / (float)HealthChunkCount);
        _lastChunk = HealthChunkCount;
    }

    public void Update(double elapsedTime)
    {
    }

    public void TookDamage(IEntity attacker, uint damage)
    {
        var chunk = (uint)(_entity.Health / (float)_chunkSize);
        if (chunk < _lastChunk && _entity.Health >= 0)
        {
            _lastChunk = chunk;
            SpawnMonsters(attacker);
        }

        if (_entity.Health <= 0)
        {
            foreach (var spawnedEntity in _spawnedEntities)
            {
                if (spawnedEntity.Health > 0)
                {
                    spawnedEntity.Die();
                }
            }

            _spawnedEntities.Clear();
        }
    }

    private void SpawnMonsters(IEntity attacker)
    {
        foreach (var group in _groups)
        {
            foreach (var member in group.Members.Select(x => x.Id).Prepend(group.Leader))
            {
                var x = _entity.PositionX + RandomNumberGenerator.GetInt32(-1500, 1501);
                var y = _entity.PositionY + RandomNumberGenerator.GetInt32(-1500, 1501);

                // Create entity instance
                var monster = new MonsterEntity(_monsterManager, _dropProvider, _animationManager, _serviceProvider,
                    _entity.Map!, _logger,
                    _itemManager, member, x, y);
                _world.SpawnEntity(monster);
                _spawnedEntities.Add(monster);
                if (monster.Behaviour is SimpleBehaviour simple)
                {
                    simple.Target = attacker;
                }
            }
        }
    }

    public void OnNewNearbyEntity(IEntity entity)
    {
    }
}
