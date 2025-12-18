using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;
using EnumsNET;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Core.Timekeeping;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.Types.Combat;
using QuantumCore.API.Game.Types.Entities;
using QuantumCore.API.Game.Types.Monsters;
using QuantumCore.API.Game.Types.Players;
using QuantumCore.API.Game.World;
using QuantumCore.API.Game.World.AI;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.World.AI;

public class SimpleBehaviour : IBehaviour
{
    private readonly IMonsterManager _monsterManager;
    private MonsterData? _proto;
    private IEntity? _entity;
    private TimeSpan _nextMovementIn;

    private int _spawnX;
    private int _spawnY;

    private TimeSpan _attackCooldown;
    private int _lastAttackX;
    private int _lastAttackY;
    private ServerTimestamp? _lastAttackTime;
    private ServerTimestamp? _lastChangeAttackPositionTime;

    public IEntity? Target { get; set; }
    private readonly Dictionary<uint, uint> _damageMap = new();

    public bool IsAggressive { get; set; }

    // mob idle wander
    private const int MOVE_MIN_DISTANCE = 300;
    private const int MOVE_MAX_DISTANCE = 700;
        
    private const int MAX_POSITION_ATTEMPTS = 16;
        
    private const long RETURN_TIMEOUT_MS = 15000;
    private const double RETURN_DISTANCE = 5000; // return to spawn if last attack >50m away
    private const double GIVE_UP_DISTANCE = 4000; // stop chase if target >40m away
        
    private const long CHANGE_ATTACK_POSITION_TIME_NEAR_MS = 10000;
    private const long CHANGE_ATTACK_POSITION_TIME_FAR_MS = 1000;
    private const double CHANGE_ATTACK_POSITION_DISTANCE = 100;
        
    private const double PREFERRED_ATTACK_RANGE_PERCENTAGE_RANGED = 0.8;
    private const double PREFERRED_ATTACK_RANGE_PERCENTAGE = 0.9;

    public SimpleBehaviour(IMonsterManager monsterManager)
    {
        _monsterManager = monsterManager;
        CalculateNextMovement();
    }

    public void Init(IEntity entity)
    {
        Debug.Assert(_entity is null);
        _entity = entity;

        _proto = _monsterManager.GetMonster(_entity.EntityClass);

        _spawnX = entity.PositionX;
        _spawnY = entity.PositionY;
        IsAggressive = entity is MonsterEntity mob && mob.Proto.AiFlag.HasAnyFlags(EAiFlags.AGGRESSIVE);
        _lastAttackTime = null;
        _lastChangeAttackPositionTime = null;
    }

    private void CalculateNextMovement()
    {
        var delayMs = RandomNumberGenerator.GetInt32(10000, 20000);
        _nextMovementIn = TimeSpan.FromMilliseconds(delayMs);
    }

    private void MoveToRandomLocation(ServerTimestamp startAt)
    {
        if (_entity is null) return;

        for (var attempt = 0; attempt < MAX_POSITION_ATTEMPTS; attempt++)
        {
            var distance = RandomNumberGenerator.GetInt32(MOVE_MIN_DISTANCE, MOVE_MAX_DISTANCE + 1);
            var (angleDx, angleDy) = MathUtils.GetDeltaByDegree(RandomNumberGenerator.GetInt32(0, 360));

            var delta = new Vector2((float)(distance * angleDx), (float)(distance * angleDy));

            if (TryGoto(_entity.Coordinates() + delta, startAt))
            {
                return;
            }
        }
    }

    /// <summary>
    /// Moves the monster in attack range to the given target
    /// </summary>
    /// <param name="target"></param>
    /// <param name="ctx"></param>
    private void MoveTo(IEntity target, TickContext ctx)
    {
        if (_entity is null || _proto is null) return;

        // We're moving to a distance of half of our attack range so we do not have to directly move again
        // TODO: predict target movement (speed/heading) before selecting the approach point
        double directionX = target.PositionX - _entity.PositionX;
        double directionY = target.PositionY - _entity.PositionY;
        var directionLength = Math.Sqrt(directionX * directionX + directionY * directionY);

        var minDistance = GetPreferredApproachDistance();
        if (directionLength <= minDistance) return;

        directionX /= directionLength;
        directionY /= directionLength;

        if (_entity is MonsterEntity { Rank: < EMonsterLevel.BOSS } &&
            ShouldChangeAttackPosition(directionLength, ctx))
        {
            if (TryChangeAttackPosition(target, directionLength, minDistance, ctx.Timestamp))
            {
                return;
            }
        }

        var baseRange = Math.Max(_proto.AttackRange * 0.75, 50);

        var targetDelta = new Vector2((float)(directionX * baseRange), (float)(directionY * baseRange));
        if (TryGoto(target.Coordinates() + targetDelta, ctx.Timestamp))
        {
            return;
        }

        var stepDelta = new Vector2((float)(directionX * (directionLength - minDistance)), (float)(directionY * (directionLength - minDistance)));
        TryGoto(target.Coordinates() + stepDelta, ctx.Timestamp);
    }

    public void Update(TickContext ctx)
    {
        if (_entity is null || _proto is null)
        {
            return;
        }

        if (Target is not null)
        {
            var targetLost = Target.Dead || Target.Map != _entity.Map;

            if (!targetLost && GIVE_UP_DISTANCE <= MathUtils.Distance(_entity.PositionX, _entity.PositionY,
                    Target.PositionX, Target.PositionY))
            {
                targetLost = true;
            }

            if (!targetLost && _lastAttackTime.HasValue)
            {
                if (ctx.ElapsedSince(_lastAttackTime.Value) > TimeSpan.FromMilliseconds(RETURN_TIMEOUT_MS))
                {
                    if (_proto.AttackRange < _entity.DistanceTo(Target))
                    {
                        targetLost = true;
                    }

                    if (!targetLost)
                    {
                        if (RETURN_DISTANCE <= MathUtils.Distance(_entity.PositionX, _entity.PositionY,
                                _lastAttackX, _lastAttackY))
                        {
                            targetLost = true;
                        }
                    }
                }
            }

            if (targetLost)
            {
                // TODO: (but not here) restore aggro if mob is from metin stone and stone is attacked again
                // Switch to next target if available
                _damageMap.Remove(Target.Vid);
                Target = NextTarget();

                if (Target is null)
                {
                    _lastAttackTime = null;
                    ResetChangeAttackPositionTimer(ctx);
                    TryGoto(new Coordinates((uint)_spawnX, (uint)_spawnY), ctx.Timestamp);
                }
            }

            if (Target is not null)
            {
                if (_entity.State == EEntityState.MOVING)
                {
                    // Check if current movement goal is in attack range of our target
                    var movementDistance = MathUtils.Distance(_entity.TargetPositionX, _entity.TargetPositionY,
                        Target.PositionX, Target.PositionY);
                    if (movementDistance > _proto.AttackRange)
                    {
                        MoveTo(Target, ctx);
                    }
                }
                else
                {
                    // Check if we can potentially attack or not
                    if (_proto.AttackRange < _entity.DistanceTo(Target))
                    {
                        MoveTo(Target, ctx);
                    }
                    else
                    {
                        _attackCooldown -= ctx.Delta;
                        if (_attackCooldown <= TimeSpan.Zero)
                        {
                            Attack(Target, ctx);
                            _attackCooldown += TimeSpan.FromSeconds(2); // todo use attack speed
                        }
                    }
                }
            }
        }

        if (_entity.State == EEntityState.IDLE)
        {
            _nextMovementIn -= ctx.Delta;

            if (_nextMovementIn <= TimeSpan.Zero)
            {
                // Move to random location
                MoveToRandomLocation(ctx.Timestamp);
                CalculateNextMovement();
            }
        }
    }

    private bool TryGoto(Coordinates target, ServerTimestamp startAt)
    {
        if (_entity is null)
        {
            return false;
        }

        if (_entity.Map is not Map localMap)
        {
            _entity.Goto((int)target.X, (int)target.Y, startAt);
            return true;
        }

        if (!localMap.IsPositionInside((int)target.X, (int)target.Y))
        {
            return false;
        }

        if (localMap.IsAttr(target, EMapAttributes.BLOCK | EMapAttributes.OBJECT))
        {
            return false;
        }

        if (_entity.IsAttrOnStraightPathTo(target, EMapAttributes.BLOCK | EMapAttributes.OBJECT))
        {
            return false;
        }

        _entity.Goto((int)target.X, (int)target.Y, startAt);
        return true;
    }

    private bool TryChangeAttackPosition(IEntity target, double currentDistance, double approachDistance,
        ServerTimestamp now)
    {
        if (_entity is null)
        {
            return false;
        }

        _lastChangeAttackPositionTime = now;

        var rotationFromTarget = MathUtils.Rotation(_entity.PositionX - target.PositionX,
            _entity.PositionY - target.PositionY);

        for (var attempt = 0; attempt < MAX_POSITION_ATTEMPTS; attempt++)
        {
            double angle;
            if (currentDistance < 500.0)
            {
                // apply a slight rotation by summing two random numbers (statistically they should be close)
                angle = rotationFromTarget + RandomNumberGenerator.GetInt32(-90, 91) + RandomNumberGenerator.GetInt32(-90, 91);
            }
            else
            {
                angle = RandomNumberGenerator.GetInt32(0, 360);
            }

            var (angleDx, angleDy) = MathUtils.GetDeltaByDegree(angle);
            var delta = new Vector2((float)(approachDistance * angleDx), (float)(approachDistance * angleDy));

            if (TryGoto(target.Coordinates() + delta, now))
            {
                return true;
            }
        }

        return false;
    }

    private bool ShouldChangeAttackPosition(double currentDistance, TickContext ctx)
    {
        if (_entity is not MonsterEntity mob)
        {
            return true;
        }

        TimeSpan changeInterval;
        if (currentDistance > CHANGE_ATTACK_POSITION_DISTANCE + mob.Proto.AttackRange)
        {
            changeInterval = TimeSpan.FromMilliseconds(CHANGE_ATTACK_POSITION_TIME_FAR_MS);
        }
        else
        {
            changeInterval = TimeSpan.FromMilliseconds(CHANGE_ATTACK_POSITION_TIME_NEAR_MS);
        }

        return ctx.ElapsedSince(_lastChangeAttackPositionTime) > changeInterval;
    }

    private double GetPreferredApproachDistance()
    {
        if (_proto is null)
        {
            return 0;
        }

        var multiplier = _proto.BattleType switch
        {
            EBattleType.RANGE or EBattleType.MAGIC => PREFERRED_ATTACK_RANGE_PERCENTAGE_RANGED, // archers and wizards attack from 80% of their range
            _ => PREFERRED_ATTACK_RANGE_PERCENTAGE
        };
        return _proto.AttackRange * multiplier;
    }

    private void ResetChangeAttackPositionTimer(TickContext ctx)
    {
        _lastChangeAttackPositionTime =
            ctx.Rewind(TimeSpan.FromMilliseconds(CHANGE_ATTACK_POSITION_TIME_NEAR_MS));
    }

    private void Attack(IEntity victim, TickContext ctx)
    {
        if (_entity is not MonsterEntity monster)
        {
            return;
        }

        monster.Rotation =
            (float)MathUtils.Rotation(victim.PositionX - monster.PositionX, victim.PositionY - monster.PositionY);

        monster.Attack(victim);

        // Send attack packet
        var packet = new CharacterMoveOut
        {
            MovementType = CharacterMovementType.ATTACK,
            Rotation = (byte)(monster.Rotation / 5),
            Vid = monster.Vid,
            PositionX = monster.PositionX,
            PositionY = monster.PositionY,
            Time = (uint)ctx.TotalElapsed.TotalMilliseconds
        };
        foreach (var entity in monster.NearbyEntities)
        {
            if (entity is PlayerEntity player)
            {
                player.Connection.Send(packet);
            }
        }
    }

    private IEntity? NextTarget()
    {
        if (_entity is null) return null;

        IEntity? target = null;
        uint maxDamage = 0;
        foreach (var (vid, damage) in _damageMap)
        {
            if (damage > maxDamage)
            {
                var attacker = _entity.Map?.GetEntity(vid);
                if (attacker is not null)
                {
                    target = attacker;
                    maxDamage = damage;
                }
            }
        }

        return target;
    }

    public void TookDamage(IEntity attacker, uint damage)
    {
        if (_entity is null) return;

        _lastAttackTime = (_entity.Map as Map)!.Clock.Now;
        _lastAttackX = _entity.PositionX;
        _lastAttackY = _entity.PositionY;

        if (!_damageMap.ContainsKey(attacker.Vid))
        {
            _damageMap[attacker.Vid] = damage;
        }
        else
        {
            _damageMap[attacker.Vid] += damage;
        }

        // Check if target has to be changed
        // TODO: track aggro separately, apply attack-type multipliers, delay swaps
        if (Target?.Map != _entity.Map)
        {
            Target = attacker;
            return;
        }

        if (Target is null) return;
        if (Target.Vid == attacker.Vid)
        {
            return;
        }

        if (_damageMap[Target.Vid] < _damageMap[attacker.Vid])
        {
            Target = attacker;
        }
    }

    public void OnNewNearbyEntity(IEntity entity)
    {
        if (IsAggressive && entity is IPlayerEntity && Target is null)
        {
            // TODO: respect stealth/invisibility affects, aggressive sight radius gating before locking
            Target = entity;
        }
    }
}
