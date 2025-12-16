using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types.Combat;
using QuantumCore.API.Game.Types.Entities;
using QuantumCore.API.Game.Types.Monsters;
using QuantumCore.API.Game.Types.Players;
using QuantumCore.API.Game.World;
using QuantumCore.API.Game.World.AI;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Packets;
using QuantumCore.Game.Services;
using QuantumCore.Game.World.AI;

namespace QuantumCore.Game.World.Entities;

public class MonsterEntity : Entity
{
    private readonly IDropProvider _dropProvider;
    private readonly ILogger _logger;
    public override EEntityType Type => EEntityType.MONSTER;
    public bool IsStone => Proto.Type == (byte)EEntityType.METIN_STONE;
    public EMonsterLevel Rank => (EMonsterLevel)Proto.Rank;

    public override IEntity? Target
    {
        get
        {
            return (_behaviour as SimpleBehaviour)?.Target;
        }
        set
        {
            if (_behaviour is SimpleBehaviour sb)
            {
                sb.Target = value;
            }
        }
    }

    public IBehaviour? Behaviour
    {
        get { return _behaviour; }
        set
        {
            _behaviour = value;
            _behaviourInitialized = false;
        }
    }

    public override byte HealthPercentage
    {
        get { return (byte)(Math.Min(Math.Max(Health / (double)Proto.Hp, 0), 1) * 100); }
    }

    public MonsterData Proto { get; private set; }

    public MonsterGroup? Group { get; set; }

    private IBehaviour? _behaviour;
    private bool _behaviourInitialized;
    private double _deadTime = 5000;
    private readonly IMap _map;
    private readonly IItemManager _itemManager;
    private IServiceProvider _serviceProvider;

    public MonsterEntity(IMonsterManager monsterManager, IDropProvider dropProvider,
        IAnimationManager animationManager,
        IServiceProvider serviceProvider,
        IMap map, ILogger logger, IItemManager itemManager, uint id, int x, int y, float rotation = 0)
        : base(animationManager, map.World.GenerateVid())
    {
        var proto = monsterManager.GetMonster(id);

        if (proto is null)
        {
            // todo handle better
            throw new InvalidOperationException($"Could not find mob proto for ID {id}. Cannot create mob entity");
        }

        _map = map;
        _dropProvider = dropProvider;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _itemManager = itemManager;
        Proto = proto;
        PositionX = x;
        PositionY = y;
        Rotation = rotation;

        MovementSpeed = (byte)Proto.MoveSpeed;

        Health = Proto.Hp;
        EntityClass = id;

        if (Proto.Type == (byte)EEntityType.MONSTER)
        {
            // it's a monster
            _behaviour = new SimpleBehaviour(monsterManager);
        }
        else if (Proto.Type == (byte)EEntityType.NPC)
        {
            // npc
        }
        else if (Proto.Type == (byte)EEntityType.METIN_STONE)
        {
            _behaviour = ActivatorUtilities.CreateInstance<StoneBehaviour>(_serviceProvider);
        }
    }

    public override void Update(double elapsedTime)
    {
        if (Map is null) return;
        if (Dead)
        {
            _deadTime -= elapsedTime;
            if (_deadTime <= 0)
            {
                Map.DespawnEntity(this);
            }
        }

        if (!_behaviourInitialized)
        {
            _behaviour?.Init(this);
            _behaviourInitialized = true;
        }

        if (!Dead)
        {
            _behaviour?.Update(elapsedTime);
        }

        base.Update(elapsedTime);
    }

    public override void Goto(int x, int y)
    {
        Rotation = (float)MathUtils.Rotation(x - PositionX, y - PositionY);

        base.Goto(x, y);
        // Send movement to nearby players
        var movement = new CharacterMoveOut
        {
            Vid = Vid,
            Rotation = (byte)(Rotation / 5),
            Argument = (byte)CharacterMovementType.WAIT,
            PositionX = TargetPositionX,
            PositionY = TargetPositionY,
            Time = (uint)GameServer.Instance.ServerTime,
            Duration = MovementDuration
        };

        foreach (var entity in NearbyEntities)
        {
            if (entity is PlayerEntity player)
            {
                player.Connection.Send(movement);
            }
        }
    }

    public override EBattleType GetBattleType()
    {
        return Proto.BattleType;
    }

    public override int GetMinDamage()
    {
        return (int)Proto.DamageRange[0];
    }

    public override int GetMaxDamage()
    {
        return (int)Proto.DamageRange[1];
    }

    public override int GetBonusDamage()
    {
        return 0; // monster don't have bonus damage as players have from their weapon
    }

    public override int Damage(IEntity attacker, EDamageType damageType, int damage)
    {
        damage = base.Damage(attacker, damageType, damage);

        if (damage >= 0)
        {
            Behaviour?.TookDamage(attacker, (uint)damage);
            Group?.TriggerAll(attacker, this);
        }

        return damage;
    }

    public void Trigger(IEntity attacker)
    {
        Behaviour?.TookDamage(attacker, 0);
    }

    public override void AddPoint(EPoint point, int value)
    {
    }

    public override void SetPoint(EPoint point, uint value)
    {
    }

    public override uint GetPoint(EPoint point)
    {
        switch (point)
        {
            case EPoint.LEVEL:
                return Proto.Level;
            case EPoint.DX:
                return Proto.Dx;
            case EPoint.ATTACK_GRADE:
                return (uint)(Proto.Level * 2 + Proto.St * 2);
            case EPoint.DEFENCE_GRADE:
                return (uint)(Proto.Level + Proto.Ht + Proto.Defence);
            case EPoint.DEFENCE_BONUS:
                return 0;
            case EPoint.EXPERIENCE:
                return Proto.Experience;
        }

        _logger.LogWarning("Point {Point} is not implemented on monster", point);
        return 0;
    }

    public override void Die()
    {
        if (Dead)
        {
            return;
        }

        CalculateDrops();

        base.Die();

        var dead = new CharacterDead {Vid = Vid};
        foreach (var entity in NearbyEntities)
        {
            if (entity is PlayerEntity player)
            {
                player.Connection.Send(dead);
            }
        }
    }

    private void CalculateDrops()
    {
        // no drops if no killer
        if (LastAttacker is null) return;

        var drops = new List<ItemInstance>();

        var (delta, range) = _dropProvider.CalculateDropPercentages(LastAttacker, this);

        // Common drops (common_drop_item.txt)
        drops.AddRange(_dropProvider.CalculateCommonDropItems(LastAttacker, this, delta, range));

        // Drop Item Group (mob_drop_item.txt)
        drops.AddRange(_dropProvider.CalculateDropItemGroupItems(this, delta, range));

        // Mob Drop Item Group (mob_drop_item.txt)
        drops.AddRange(_dropProvider.CalculateMobDropItemGroupItems(LastAttacker, this, delta, range));

        // Level drops (mob_drop_item.txt)
        drops.AddRange(_dropProvider.CalculateLevelDropItems(LastAttacker, this, delta, range));

        // Etc item drops (etc_drop_item.txt)
        drops.AddRange(_dropProvider.CalculateEtcDropItems(this, delta, range));

        if (IsStone)
        {
            // Spirit stone drops
            drops.AddRange(_dropProvider.CalculateMetinDropItems(this, delta, range));
        }

        // todo:
        // - horse riding skill drops
        // - quest item drops
        // - event item drops

        // Finally, drop the items
        foreach (var drop in drops)
        {
            // todo: if drop is yang, adjust the amount in function below instead of '1'
            _map.AddGroundItem(drop, PositionX, PositionY, 1, LastAttacker.Name);
        }
    }

    protected override void OnNewNearbyEntity(IEntity entity)
    {
        _behaviour?.OnNewNearbyEntity(entity);
    }

    protected override void OnRemoveNearbyEntity(IEntity entity)
    {
    }

    public override void OnDespawn()
    {
        if (Group is not null)
        {
            Group.Monsters.Remove(this);
            if (Group.Monsters.Count == 0)
            {
                (Map as Map)?.EnqueueGroupRespawn(Group);
            }
        }
    }

    public override void ShowEntity(IConnection connection)
    {
        if (Dead)
        {
            return; // no need to send dead entities to new players
        }

        connection.Send(new SpawnCharacter
        {
            Vid = Vid,
            CharacterType = (EEntityType)Proto.Type,
            Angle = Rotation,
            PositionX = PositionX,
            PositionY = PositionY,
            Class = (ushort)Proto.Id,
            MoveSpeed = (byte)Proto.MoveSpeed,
            AttackSpeed = (byte)Proto.AttackSpeed
        });

        if (Proto.Type == (byte)EEntityType.NPC)
        {
            // NPCs need additional information too to show up for some reason
            connection.Send(new CharacterInfo
            {
                Vid = Vid, Empire = Proto.Empire, Level = 0, Name = Proto.TranslatedName
            });
        }
    }

    public override void HideEntity(IConnection connection)
    {
        connection.Send(new RemoveCharacter {Vid = Vid});
    }


    public override string ToString()
    {
        return $"{Proto.TranslatedName?.Trim((char)0x00)} ({Proto.Id})";
    }
}
