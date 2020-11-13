using QuantumCore.Core.Networking;
using QuantumCore.Database;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.World
{
    public class PlayerEntity
    {
        public Connection Connection { get; }
        public Player Player { get; private set; }
        
        public int PositionX { get; set; }
        public int PositionY { get; set; }

        public PlayerEntity(Player player, Connection connection)
        {
            Connection = connection;
            Player = player;
            PositionX = player.PositionX;
            PositionY = player.PositionY;
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