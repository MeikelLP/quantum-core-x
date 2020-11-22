using QuantumCore.Core.Networking;
using QuantumCore.Database;
using QuantumCore.Game.Packets;
using Serilog;

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

        public override void Update(double elapsedTime)
        {
            if (Map == null) return; // We don't have a map yet so we aren't spawned
            
            base.Update(elapsedTime);
        }

        protected override void OnNewNearbyEntity(Entity entity)
        {
            Log.Debug($"New entity {entity} nearby {this}");
            entity.ShowEntity(Connection);
        }

        protected override void OnRemoveNearbyEntity(Entity entity)
        {
            Log.Debug($"Remove entity {entity} nearby {this}");
            Connection.Send(new RemoveCharacter
            {
                Vid = entity.Vid
            });
        }

        public override void ShowEntity(Connection connection)
        {
            SendCharacter(connection);
            SendCharacterAdditional(connection);
        }

        public override string ToString()
        {
            return Player.Name + "(Player)";
        }

        public void SendBasicData()
        {
            var details = new CharacterDetails
            {
                Vid = Vid,
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
                Vid = Vid,
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
                Vid = Vid, // todo
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