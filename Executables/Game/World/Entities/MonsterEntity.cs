using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.API.Game.World.AI;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World.AI;

namespace QuantumCore.Game.World.Entities
{
    public class MonsterEntity : Entity
    {
        private readonly ILogger _logger;
        public override EEntityType Type => EEntityType.Monster;

        public IBehaviour Behaviour {
            get { return _behaviour; }
            set {
                _behaviour = value;
                _behaviourInitialized = false;
            }
        }

        public override byte HealthPercentage {
            get {
                return (byte)(Math.Min(Math.Max(Health / (double)_proto.Hp, 0), 1) * 100);
            }
        }

        public MonsterData Proto { get { return _proto; } }

        public MonsterGroup Group { get; set; }

        private readonly MonsterData _proto;
        private IBehaviour _behaviour;
        private bool _behaviourInitialized;
        private double _deadTime = 5000;

        public MonsterEntity(IMonsterManager monsterManager, IAnimationManager animationManager, IWorld world, ILogger logger, uint id, int x, int y, float rotation = 0)
            : base(animationManager, world.GenerateVid())
        {
            _logger = logger;
            _proto = monsterManager.GetMonster(id);
            PositionX = x;
            PositionY = y;
            Rotation = rotation;

            MovementSpeed = (byte) _proto.MoveSpeed;

            Health = _proto.Hp;
            EntityClass = id;

            if (_proto.Type == (byte) EEntityType.Monster)
            {
                // it's a monster
                _behaviour = new SimpleBehaviour(monsterManager);
            }
            else if(_proto.Type == (byte) EEntityType.Npc)
            {
                // npc
            }
        }

        public override void Update(double elapsedTime)
        {
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
            Rotation = (float) MathUtils.Rotation(x - PositionX, y - PositionY);

            base.Goto(x, y);

            // Send movement to nearby players
            var movement = new CharacterMoveOut {
                Vid = Vid,
                Rotation = (byte) (Rotation / 5),
                Argument = (byte) CharacterMove.CharacterMovementType.Wait,
                PositionX = TargetPositionX,
                PositionY = TargetPositionY,
                Time = (uint) GameServer.Instance.ServerTime,
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

        public override byte GetBattleType()
        {
            return _proto.BattleType;
        }

        public override int GetMinDamage()
        {
            return (int)_proto.DamageRange[0];
        }

        public override int GetMaxDamage()
        {
            return (int)_proto.DamageRange[1];
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
                Behaviour?.TookDamage(attacker, (uint) damage);
                Group?.TriggerAll(attacker, this);
            }

            return damage;
        }

        public void Trigger(IEntity attacker)
        {
            Behaviour?.TookDamage(attacker, 0);
        }

        public override void AddPoint(EPoints point, int value)
        {
        }

        public override void SetPoint(EPoints point, uint value)
        {
        }

        public override uint GetPoint(EPoints point)
        {
            switch (point)
            {
                case EPoints.Level:
                    return _proto.Level;
                case EPoints.Dx:
                    return _proto.Dx;
                case EPoints.AttackGrade:
                    return (uint) (_proto.Level * 2 + _proto.St * 2);
                case EPoints.DefenceGrade:
                    return (uint)(_proto.Level + _proto.Ht + _proto.Defence);
                case EPoints.DefenceBonus:
                    return 0;
                case EPoints.Experience:
                    return _proto.Experience;
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

            base.Die();

            var dead = new CharacterDead { Vid = Vid };
            foreach (var entity in NearbyEntities)
            {
                if (entity is PlayerEntity player)
                {
                    player.Connection.Send(dead);
                }
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
            if (Group != null)
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
                CharacterType = _proto.Type,
                Angle = Rotation,
                PositionX = PositionX,
                PositionY = PositionY,
                Class = (ushort) _proto.Id,
                MoveSpeed = (byte) _proto.MoveSpeed,
                AttackSpeed = (byte) _proto.AttackSpeed
            });

            if (_proto.Type == (byte) EEntityType.Npc)
            {
                // NPCs need additional information too to show up for some reason
                connection.Send(new CharacterInfo {
                    Vid = Vid,
                    Empire = _proto.Empire,
                    Level = _proto.Level,
                    Name = _proto.TranslatedName
                });
            }
        }

        public override void HideEntity(IConnection connection)
        {
            connection.Send(new RemoveCharacter
            {
                Vid = Vid
            });
        }

        public override string ToString()
        {
            return $"{_proto.TranslatedName?.Trim((char) 0x00)} ({_proto.Id})";
        }
    }
}
