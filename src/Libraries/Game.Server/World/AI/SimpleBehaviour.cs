using System.Diagnostics;
using System.Security.Cryptography;
using EnumsNET;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.API.Game.World.AI;
using QuantumCore.Core.Utils;
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

        public IEntity? Target { get; set; }
        private readonly Dictionary<uint, uint> _damageMap = new();

        public bool IsAggressive { get; set; }

        private const int MoveRadius = 1000;

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
        }

        private void CalculateNextMovement()
        {
            _nextMovementIn = RandomNumberGenerator.GetInt32(10000, 20000);
        }

        private void MoveToRandomLocation()
        {
            if (_entity is null) return;

            var offsetX = RandomNumberGenerator.GetInt32(-MoveRadius, MoveRadius);
            var offsetY = RandomNumberGenerator.GetInt32(-MoveRadius, MoveRadius);

            var targetX = _spawnX + offsetX;
            var targetY = _spawnY + offsetY;

            _entity.Goto(targetX, targetY);
        }

        /// <summary>
        /// Moves the monster in attack range to the given target
        /// </summary>
        /// <param name="target"></param>
        private void MoveTo(IEntity target)
        {
            if (_entity is null || _proto is null) return;

            // We're moving to a distance of half of our attack range so we do not have to directly move again
            double directionX = target.PositionX - _entity.PositionX;
            double directionY = target.PositionY - _entity.PositionY;
            var directionLength = Math.Sqrt(directionX * directionX + directionY * directionY);
            directionX /= directionLength;
            directionY /= directionLength;

            var targetPositionX = target.PositionX + directionX * _proto.AttackRange * 0.75;
            var targetPositionY = target.PositionY + directionY * _proto.AttackRange * 0.75;

            _entity.Goto((int)targetPositionX, (int)targetPositionY);
        }

        public void Update(double elapsedTime)
        {
            if (_entity is null || _proto is null)
            {
                return;
            }

            if (Target != null)
            {
                if (Target.Dead || Target.Map != _entity.Map)
                {
                    // Switch to next target if available
                    _damageMap.Remove(Target.Vid);
                    Target = NextTarget();
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
            if (!_damageMap.ContainsKey(attacker.Vid))
            {
                _damageMap[attacker.Vid] = damage;
            }
            else
            {
                _damageMap[attacker.Vid] += damage;
            }

            // Check if target has to be changed
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
                Target = entity;
                // TODO stop following at some point
            }
        }
    }
}
