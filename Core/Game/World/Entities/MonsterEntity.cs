using System;
using System.Security.Cryptography;
using QuantumCore.API;
using QuantumCore.API.Game.World;
using QuantumCore.API.Game.World.AI;
using QuantumCore.Core.Networking;
using QuantumCore.Core.Types;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World.AI;
using Serilog;

namespace QuantumCore.Game.World.Entities
{
    public class MonsterEntity : Entity, IDamageable
    {
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
                Log.Debug($"Health Percentage of {Vid}");
                return (byte)(Math.Min(Math.Max(Health / (double)_proto.Hp, 0), 1) * 100);
            }
        }

        private readonly MobProto.Monster _proto;
        private IBehaviour _behaviour;
        private bool _behaviourInitialized;
        private double _deadTime = 5000;
        
        public MonsterEntity(uint id, int x, int y, float rotation = 0) : base(World.Instance.GenerateVid())
        {
            _proto = MonsterManager.GetMonster(id);
            PositionX = x;
            PositionY = y;
            Rotation = rotation;

            MovementSpeed = (byte) _proto.MoveSpeed;
            
            Health = _proto.Hp;
            EntityClass = id;

            _behaviour = new SimpleBehaviour();
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
            
            _behaviour?.Update(elapsedTime);
            
            base.Update(elapsedTime);
        }

        public override void Goto(int x, int y)
        {
            Rotation = (float) MathUtils.Rotation(PositionX - x, PositionY - y);
            
            base.Goto(x, y);
            
            // Send movement to nearby players
            var movement = new CharacterMoveOut {
                Vid = Vid,
                Rotation = (byte) (Rotation / 5),
                Argument = (byte) CharacterMove.CharacterMovementType.Wait,
                PositionX = TargetPositionX,
                PositionY = TargetPositionY,
                Time = (uint) GameServer.Instance.Server.ServerTime,
                Duration = MovementDuration
            };
            ForEachNearbyEntity(entity =>
            {
                if (entity is PlayerEntity player)
                {
                    player.Connection.Send(movement);
                }
            });
        }

        public override void Die()
        {
            if (Dead)
            {
                return;
            }
            
            base.Die();

            var dead = new CharacterDead { Vid = Vid };
            ForEachNearbyEntity(entity =>
            {
                if (entity is PlayerEntity player)
                {
                    player.Connection.Send(dead);
                }
            });
        }

        protected override void OnNewNearbyEntity(IEntity entity)
        {
        }

        protected override void OnRemoveNearbyEntity(IEntity entity)
        {
        }

        public override void OnDespawn()
        {
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
                CharacterType = (byte) EEntityType.Monster,
                Angle = Rotation,
                PositionX = PositionX,
                PositionY = PositionY,
                Class = (ushort) _proto.Id,
                MoveSpeed = (byte) _proto.MoveSpeed,
                AttackSpeed = (byte) _proto.AttackSpeed
            });
        }

        public long TakeDamage(long damage, Entity attacker)
        {
            Health -= damage;

            foreach (var player in TargetedBy)
            {
                player.SendTarget();
            }

            if (Health <= 0)
            {
                Health = 0;
                Die();
            }

            return damage;
        }

        public uint GetDefence()
        {
            return _proto.Defence;
        }

        public override string ToString()
        {
            return $"{_proto.TranslatedName} ({_proto.Id})";
        }
    }
}