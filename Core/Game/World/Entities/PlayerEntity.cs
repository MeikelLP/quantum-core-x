using QuantumCore.Core.Networking;
using QuantumCore.Database;
using QuantumCore.Game.Packets;
using Serilog;

namespace QuantumCore.Game.World.Entities
{
    public class PlayerEntity : Entity
    {
        private enum State
        {
            Idle,
            Moving
        }
        
        public GameConnection Connection { get; }
        public Player Player { get; private set; }
        
        public uint MovementDuration { get; private set; }

        private int _targetX;
        private int _startX;
        private int _targetY;
        private int _startY;
        private long _movementStart;
        private State _state = State.Idle;
        
        public PlayerEntity(Player player, GameConnection connection) : base(World.Instance.GenerateVid())
        {
            Connection = connection;
            Player = player;
            PositionX = player.PositionX;
            PositionY = player.PositionY;
        }

        public void Goto(int x, int y)
        {
            if (PositionX == x && PositionY == y) return;
            if (_targetX == x && _targetY == y) return;

            _state = State.Moving;
            _targetX = x;
            _targetY = y;
            _startX = PositionX;
            _startY = PositionY;
            _movementStart = Connection.Server.ServerTime;
            
            // todo calculate movement duration
            MovementDuration = 0;
        }

        public override void Update(double elapsedTime)
        {
            if (Map == null) return; // We don't have a map yet so we aren't spawned

            if (_state == State.Moving)
            {
                var elapsed = Connection.Server.ServerTime - _movementStart;
                var rate = elapsed / (float) MovementDuration;
                if (rate > 1) rate = 1;

                var x = (int)((_targetX - _startX) * rate + _startX);
                var y = (int)((_targetY - _startY) * rate + _startY);

                PositionX = x;
                PositionY = y;

                if (rate >= 1)
                {
                    _state = State.Idle;
                    Log.Debug($"Movement of player {Player.Name} done");
                }
            }

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