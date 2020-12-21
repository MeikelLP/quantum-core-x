using System.Diagnostics;
using System.Security.Cryptography;
using QuantumCore.API.Game.World;
using QuantumCore.API.Game.World.AI;
using Serilog;

namespace QuantumCore.Game.World.AI
{
    public class SimpleBehaviour : IBehaviour
    {
        private IEntity _entity;
        private long _nextMovementIn;

        private int _spawnX;
        private int _spawnY;
        
        private const int MoveRadius = 1000;

        public SimpleBehaviour()
        {
            CalculateNextMovement();
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
        
        public void Init(IEntity entity)
        {
            Debug.Assert(_entity == null);
            _entity = entity;

            _spawnX = entity.PositionX;
            _spawnY = entity.PositionY;
        }

        public void Update(double elapsedTime)
        {
            if (_entity == null)
            {
                return;
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

        public void TookDamage(IEntity attacker, uint damage)
        {
            
        }

        public void OnNewNearbyEntity(IEntity entity)
        {
            // todo implement aggressive flag
        }
    }
}