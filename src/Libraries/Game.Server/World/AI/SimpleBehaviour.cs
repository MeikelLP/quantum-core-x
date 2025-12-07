using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;
using EnumsNET;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.Types.Combat;
using QuantumCore.API.Game.World;
using QuantumCore.API.Game.World.AI;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.World.AI
{
    public class SimpleBehaviour : IBehaviour
    {
        private readonly IMonsterManager _monsterManager;
        private MonsterData? _proto;
        private IEntity? _entity;
        private long _nextMovementIn;

        private int _spawnX;
        private int _spawnY;

        private double _attackCooldown;
        private int _lastAttackX;
        private int _lastAttackY;
        private long _lastAttackTime;
        private long _lastChangeAttackPositionTime;

        public IEntity? Target { get; set; }
        private readonly Dictionary<uint, uint> _damageMap = new();

        public bool IsAggressive { get; set; }

        // mob idle wander
        private const int MoveMinDistance = 300;
        private const int MoveMaxDistance = 700;
        
        private const int MaxPositionAttempts = 16;
        
        private const long ReturnTimeoutMs = 15000;
        private const double ReturnDistance = 5000; // return to spawn if last attack >50m away
        private const double GiveUpDistance = 4000; // stop chase if target >40m away
        
        private const long ChangeAttackPositionTimeNearMs = 10000;
        private const long ChangeAttackPositionTimeFarMs = 1000;
        private const double ChangeAttackPositionDistance = 100;
        
        private const double PreferredAttackRangePercentageRanged = 0.8;
        private const double PreferredAttackRangePercentage = 0.9;

        public SimpleBehaviour(IMonsterManager monsterManager)
        {
            _monsterManager = monsterManager;
            CalculateNextMovement();
        }

        public void Init(IEntity entity)
        {
            Debug.Assert(_entity == null);
            _entity = entity;

            _proto = _monsterManager.GetMonster(_entity.EntityClass);

            _spawnX = entity.PositionX;
            _spawnY = entity.PositionY;
            IsAggressive = entity is MonsterEntity mob && mob.Proto.AiFlag.HasAnyFlags(EAiFlags.Aggressive);
            _lastAttackTime = 0;
            ResetChangeAttackPositionTimer();
        }

        private void CalculateNextMovement()
        {
            _nextMovementIn = RandomNumberGenerator.GetInt32(10000, 20000);
        }

        private void MoveToRandomLocation()
        {
            if (_entity is null) return;

            for (var attempt = 0; attempt < MaxPositionAttempts; attempt++)
            {
                var distance = RandomNumberGenerator.GetInt32(MoveMinDistance, MoveMaxDistance + 1);
                var (angleDx, angleDy) = MathUtils.GetDeltaByDegree(RandomNumberGenerator.GetInt32(0, 360));

                var delta = new Vector2((float)(distance * angleDx), (float)(distance * angleDy));

                if (TryGoto(_entity.Coordinates() + delta))
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Moves the monster in attack range to the given target
        /// </summary>
        /// <param name="target"></param>
        private void MoveTo(IEntity target)
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

            if (_entity is MonsterEntity monster && monster.Rank < EMonsterLevel.Boss &&
                ShouldChangeAttackPosition(directionLength))
            {
                if (TryChangeAttackPosition(target, directionLength, minDistance))
                {
                    return;
                }
            }

            var baseRange = Math.Max(_proto.AttackRange * 0.75, 50);

            var targetDelta = new Vector2((float)(directionX * baseRange), (float)(directionY * baseRange));
            if (TryGoto(target.Coordinates() + targetDelta))
            {
                return;
            }

            var stepDelta = new Vector2((float)(directionX * (directionLength - minDistance)), (float)(directionY * (directionLength - minDistance)));
            TryGoto(target.Coordinates() + stepDelta);
        }

        public void Update(double elapsedTime)
        {
            if (_entity is null || _proto is null)
            {
                return;
            }

            if (Target != null)
            {
                var targetLost = Target.Dead || Target.Map != _entity.Map;

                if (!targetLost && GiveUpDistance <= MathUtils.Distance(_entity.PositionX, _entity.PositionY,
                        Target.PositionX, Target.PositionY))
                {
                    targetLost = true;
                }

                if (!targetLost && _lastAttackTime > 0)
                {
                    var elapsedSinceAttack = GameServer.Instance.ServerTime - _lastAttackTime;
                    if (elapsedSinceAttack >= ReturnTimeoutMs)
                    {
                        if (_proto.AttackRange < MathUtils.Distance(_entity.PositionX, _entity.PositionY,
                                Target.PositionX, Target.PositionY))
                        {
                            targetLost = true;
                        }

                        if (!targetLost)
                        {
                            if (ReturnDistance <= MathUtils.Distance(_entity.PositionX, _entity.PositionY,
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
                        _lastAttackTime = 0;
                        ResetChangeAttackPositionTimer();
                        TryGoto(new Coordinates((uint)_spawnX, (uint)_spawnY));
                    }
                }

                if (Target != null)
                {
                    if (_entity.State == EEntityState.Moving)
                    {
                        // Check if current movement goal is in attack range of our target
                        var movementDistance = MathUtils.Distance(_entity.TargetPositionX, _entity.TargetPositionY,
                            Target.PositionX, Target.PositionY);
                        if (movementDistance > _proto.AttackRange)
                        {
                            MoveTo(Target);
                        }
                    }
                    else
                    {
                        // Check if we can potentially attack or not
                        var distance = MathUtils.Distance(_entity.PositionX, _entity.PositionY, Target.PositionX,
                            Target.PositionY);
                        if (distance > _proto.AttackRange)
                        {
                            MoveTo(Target);
                        }
                        else
                        {
                            _attackCooldown -= elapsedTime;
                            if (_attackCooldown <= 0)
                            {
                                Attack(Target);
                                _attackCooldown += 2000; // todo use attack speed
                            }
                        }
                    }
                }
            }

            if (_entity.State == EEntityState.Idle)
            {
                _nextMovementIn -= (int)elapsedTime;

                if (_nextMovementIn <= 0)
                {
                    // Move to random location
                    MoveToRandomLocation();
                    CalculateNextMovement();
                }
            }
        }

        private bool TryGoto(Coordinates target)
        {
            if (_entity is null)
            {
                return false;
            }

            if (_entity.Map is not Map localMap)
            {
                _entity.Goto((int)target.X, (int)target.Y);
                return true;
            }

            if (!localMap.IsPositionInside((int)target.X, (int)target.Y))
            {
                return false;
            }

            if (localMap.IsAttr(target, EMapAttributes.Block | EMapAttributes.Object))
            {
                return false;
            }

            if (_entity.IsAttrOnStraightPathTo(target, EMapAttributes.Block | EMapAttributes.Object))
            {
                return false;
            }

            _entity.Goto((int)target.X, (int)target.Y);
            return true;
        }

        private bool TryChangeAttackPosition(IEntity target, double currentDistance, double approachDistance)
        {
            if (_entity is null)
            {
                return false;
            }

            _lastChangeAttackPositionTime = GameServer.Instance.ServerTime;

            var rotationFromTarget = MathUtils.Rotation(_entity.PositionX - target.PositionX,
                _entity.PositionY - target.PositionY);

            for (var attempt = 0; attempt < MaxPositionAttempts; attempt++)
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

                if (TryGoto(target.Coordinates() + delta))
                {
                    return true;
                }
            }

            return false;
        }

        private bool ShouldChangeAttackPosition(double currentDistance)
        {
            if (_entity is not MonsterEntity mob)
            {
                return true;
            }

            var changeInterval = ChangeAttackPositionTimeNearMs;
            if (currentDistance > ChangeAttackPositionDistance + mob.Proto.AttackRange)
            {
                changeInterval = ChangeAttackPositionTimeFarMs;
            }

            return GameServer.Instance.ServerTime - _lastChangeAttackPositionTime > changeInterval;
        }

        private double GetPreferredApproachDistance()
        {
            if (_proto is null)
            {
                return 0;
            }

            var multiplier = _proto.BattleType switch
            {
                EBattleType.Range or EBattleType.Magic => PreferredAttackRangePercentageRanged, // archers and wizards attack from 80% of their range
                _ => PreferredAttackRangePercentage
            };
            return _proto.AttackRange * multiplier;
        }

        private void ResetChangeAttackPositionTimer()
        {
            _lastChangeAttackPositionTime = GameServer.Instance.ServerTime - ChangeAttackPositionTimeNearMs;
        }

        private void Attack(IEntity victim)
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
                MovementType = (byte)CharacterMove.CharacterMovementType.Attack,
                Rotation = (byte)(monster.Rotation / 5),
                Vid = monster.Vid,
                PositionX = monster.PositionX,
                PositionY = monster.PositionY,
                Time = (uint)GameServer.Instance.ServerTime
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
                    if (attacker != null)
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

            _lastAttackTime = GameServer.Instance.ServerTime;
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
}
