using System.Security.Cryptography;
using QuantumCore.API;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Networking;
using QuantumCore.Core.Types;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.World.Entities
{
    public class MonsterEntity : Entity
    {
        private readonly MobProto.Monster _proto;
        
        public MonsterEntity(uint id, int x, int y) : base(World.Instance.GenerateVid())
        {
            _proto = MonsterManager.GetMonster(id);
            PositionX = x;
            PositionY = y;
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
                Angle = 0,
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