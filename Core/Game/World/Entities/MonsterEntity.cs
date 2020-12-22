using System.Security.Cryptography;
using QuantumCore.API;
using QuantumCore.API.Game.World;
using QuantumCore.API.Game.World.AI;
using QuantumCore.Core.Networking;
using QuantumCore.Core.Types;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World.AI;

namespace QuantumCore.Game.World.Entities
{
    public class MonsterEntity : Entity
    {
        public override EEntityType Type => EEntityType.Monster;

        public IBehaviour Behaviour {
            get { return _behaviour; }
            set {
                _behaviour = value;
                _behaviourInitialized = false;
            }
        }

        private readonly MobProto.Monster _proto;
        private IBehaviour _behaviour;
        private bool _behaviourInitialized;
        
        public MonsterEntity(uint id, int x, int y, float rotation = 0) : base(World.Instance.GenerateVid())
        {
            _proto = MonsterManager.GetMonster(id);
            PositionX = x;
            PositionY = y;
            Rotation = rotation;

            MovementSpeed = (byte) _proto.MoveSpeed;
            EntityClass = id;

            _behaviour = new SimpleBehaviour();
        }

        public override void Update(double elapsedTime)
        {
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

        public override string ToString()
        {
            return $"{_proto.TranslatedName} ({_proto.Id})";
        }
    }
}