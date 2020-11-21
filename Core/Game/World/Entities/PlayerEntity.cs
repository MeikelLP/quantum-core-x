using QuantumCore.Core.Networking;
using QuantumCore.Database;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.World.Entities
{
    public class PlayerEntity : Entity
    {
        public Connection Connection { get; }
        public Player Player { get; private set; }

        public PlayerEntity(Player player, Connection connection) : base(World.Instance.GenerateVid())
        {
            Connection = connection;
            Player = player;
            PositionX = player.PositionX;
            PositionY = player.PositionY;
        }

        public override void Update()
        {
            if (Map == null) return; // We don't have a map yet so we aren't spawned
        }

        public void SendBasicData()
        {
            var details = new CharacterDetails
            {
                Vid = 1,
                Name = Player.Name,
                Class = Player.PlayerClass,
                PositionX = PositionX,
                PositionY = PositionY,
                Empire = 1
            };
            Connection.Send(details);
        }

        public void SendPoints()
        {
            var points = new CharacterPoints();
            Connection.Send(points);
        }

        public void SendCharacter(Connection connection)
        {
            connection.Send(new SpawnCharacter
            {
                Vid = 1, // todo
                CharacterType = 6, // todo
                Angle = 0,
                PositionX = PositionX,
                PositionY = PositionY,
                Class = Player.PlayerClass,
                MoveSpeed = 100,
                AttackSpeed = 100
            });
        }

        public void SendCharacterAdditional(Connection connection)
        {
            connection.Send(new CharacterInfo
            {
                Vid = 1, // todo
                Name = Player.Name,
                Empire = 1, // todo
                Level = Player.Level,
            });
        }

        public void Show(Connection connection)
        {
            SendCharacter(connection);
            SendCharacterAdditional(connection);
        }
    }
}