using System.Threading.Tasks;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Cache;
using QuantumCore.Core.Networking;
using QuantumCore.Core.Utils;
using QuantumCore.Database;
using QuantumCore.Game.Packets;
using QuantumCore.Game.PlayerUtils;
using Serilog;

namespace QuantumCore.Game.World.Entities
{
    public class PlayerEntity : Entity, IPlayerEntity
    {
        private enum State
        {
            Idle,
            Moving
        }

        public string Name => Player.Name;
        public GameConnection Connection { get; }
        public Player Player { get; private set; }
        public Inventory Inventory { get; private set; }
        
        public uint MovementDuration { get; private set; }

        private int _targetX;
        private int _startX;
        private int _targetY;
        private int _startY;
        private long _movementStart;
        private State _state = State.Idle;
        private byte _moveSpeed = 150;
        private byte _attackSpeed = 140;

        private const int _persistInterval = 1000;
        private int _persistTime = 0;

        public PlayerEntity(Player player, GameConnection connection) : base(World.Instance.GenerateVid())
        {
            Connection = connection;
            Player = player;
            PositionX = player.PositionX;
            PositionY = player.PositionY;
            Inventory = new Inventory(player.Id, 0, 5, 9, 2);
        }

        public async Task Load()
        {
            await Inventory.Load();
        }

        public void Goto(int x, int y)
        {
            if (PositionX == x && PositionY == y) return;
            if (_targetX == x && _targetY == y) return;

            var animation =
                AnimationManager.GetAnimation(Player.PlayerClass, AnimationType.Run, AnimationSubType.General);
            if (animation == null)
            {
                Log.Debug($"No animation for player class {Player.PlayerClass} with General/Run found!");
            }

            _state = State.Moving;
            _targetX = x;
            _targetY = y;
            _startX = PositionX;
            _startY = PositionY;
            _movementStart = Connection.Server.ServerTime;

            var distance = MathUtils.Distance(_startX, _startY, _targetX, _targetY);
            if (animation == null)
            {
                MovementDuration = 0;
            }
            else
            {
                var animationSpeed = -animation.AccumulationY / animation.MotionDuration;
                var i = 100 - _moveSpeed;
                if (i > 0)
                {
                    i = 100 + i;
                } else if (i < 0)
                {
                    i = 10000 / (100 - i);
                }
                else
                {
                    i = 100;
                }

                var duration = (int) ((distance / animationSpeed) * 1000) * i / 100;
                MovementDuration = (uint) duration;
                Log.Debug($"movement duration = {MovementDuration}");
            }
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
                    Log.Debug($"Movement of player {Player.Name} ({Player.PlayerClass}) done");
                }
            }

            _persistTime += (int)elapsedTime;
            if (_persistTime > _persistInterval)
            {
                Persist();
                _persistTime -= _persistInterval;
            }

            base.Update(elapsedTime);
        }

        private async Task Persist()
        {
            Player.PositionX = PositionX;
            Player.PositionY = PositionY;
            
            await CacheManager.Redis.Set($"player:{Player.Id}", Player);
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

        public override void OnDespawn()
        {
            Persist();
        }

        public override void ShowEntity(Connection connection)
        {
            SendCharacter(connection);
            SendCharacterAdditional(connection);
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

        public void SendInventory()
        {
            foreach (var item in Inventory.Items)
            {
                SendItem(item);
            }
        }

        private void SendItem(Item item)
        {
            var p = new SetItem {
                Window = item.Window,
                Position = (ushort)item.Position,
                ItemId = item.ItemId,
                Count = item.Count
            };
            Connection.Send(p);
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

        public override string ToString()
        {
            return Player.Name + "(Player)";
        }
    }
}