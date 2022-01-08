using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using QuantumCore.API.Game.World;
using QuantumCore.API.Game.World.AI;
using QuantumCore.Core.Types;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World.Entities;
using Serilog;

namespace QuantumCore.Game.World.AI
{
    public class SimpleBehaviour : IBehaviour
    {
        private MobProto.Monster _proto;
        private IEntity _entity;
        private long _nextMovementIn;

        private int _spawnX;
        private int _spawnY;

        private double _attackCooldown;
        private int _lastAttackX;
        private int _lastAttackY;

        private IEntity _targetEntity;
        private readonly Dictionary<uint, uint> _damageMap = new();

        private const int MoveRadius = 1000;

        public SimpleBehaviour()
        {
            CalculateNextMovement();
        }
        
        public void Init(IEntity entity)
        {
            Debug.Assert(_entity == null);
            _entity = entity;

            _proto = MonsterManager.GetMonster(_entity.EntityClass);

            _spawnX = entity.PositionX;
            _spawnY = entity.PositionY;
        }

        private void CalculateNextMovement()
        {
            _nextMovementIn = RandomNumberGenerator.GetInt32(10000, 20000);
        }

        private void MoveToRandomLocation()
        {
            var offsetX = RandomNumberGenerator.GetInt32(-MoveRadius, MoveRadius);
            var offsetY = RandomNumberGenerator.GetInt32(-MoveRadius, MoveRadius);

            var targetX = _spawnX + offsetX;
            var targetY = _spawnY + offsetY;
            
            _entity.Goto(targetX, targetY);
        }

        public void Update(double elapsedTime)
        {
            if (_entity == null)
            {
                return;
            }

            if (_targetEntity != null)
            {
                if (_targetEntity.Dead || _targetEntity.Map != _entity.Map)
                {
                    // Switch to next target if available
                    _damageMap.Remove(_targetEntity.Vid);
                    _targetEntity = NextTarget();
                }

                if (_targetEntity != null)
                {
                    if (_entity.State == EEntityState.Moving)
                    {
                        // Check if current movement goal is in attack range of our target
                        var movementDistance = MathUtils.Distance(_entity.TargetPositionX, _entity.TargetPositionY,
                            _targetEntity.PositionX, _targetEntity.PositionY);
                        if (movementDistance > _proto.AttackRange)
                        {
                            // We have to update our move goal
                            _entity.Goto(_targetEntity.PositionX,
                                _targetEntity.PositionY); // todo do not directly move onto target
                        }
                    }
                    else
                    {
                        // Check if we can potentially attack or not
                        var distance = MathUtils.Distance(_entity.PositionX, _entity.PositionY, _targetEntity.PositionX,
                            _targetEntity.PositionY);
                        if (distance > _proto.AttackRange)
                        {
                            _entity.Goto(_targetEntity.PositionX,
                                _targetEntity.PositionY); // todo do not directly move onto target
                        }
                        else
                        {
                            _attackCooldown -= elapsedTime;
                            if (_attackCooldown <= 0)
                            {
                                Attack(_targetEntity);
                                _attackCooldown += 2000; // todo use attack speed
                            }
                        }
                    }
                }
            }

            if (_entity.State == EEntityState.Idle)
            {
                _nextMovementIn -= (int) elapsedTime;

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

            monster.Attack(victim);
        }

        private IEntity NextTarget()
        {
            IEntity target = null;
            uint maxDamage = 0;
            foreach (var (vid, damage) in _damageMap)
            {
                if (damage > maxDamage)
                {
                    var attacker = _entity.Map.GetEntity(vid);
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
            if (!_damageMap.ContainsKey(attacker.Vid))
            {
                _damageMap[attacker.Vid] = damage;
            }
            else
            {
                _damageMap[attacker.Vid] += damage;
            }
            
            // Check if target has to be changed
            if (_targetEntity?.Map != _entity.Map)
            {
                _targetEntity = attacker;
                return;
            }

            if (_targetEntity.Vid == attacker.Vid)
            {
                return;
            }

            if (_damageMap[_targetEntity.Vid] < _damageMap[attacker.Vid])
            {
                _targetEntity = attacker;
            }
        }

        public void OnNewNearbyEntity(IEntity entity)
        {
            // todo implement aggressive flag
        }
    }
}