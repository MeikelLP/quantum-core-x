using System;
using System.Threading.Tasks;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.API.Game.World.AI;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World.AI;
using Serilog;

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

        public override byte HealthPercentage {
            get {
                Log.Debug($"Health Percentage of {Vid}");
                return (byte)(Math.Min(Math.Max(Health / (double)_proto.Hp, 0), 1) * 100);
            }
        }

        public MonsterData Proto { get { return _proto; } }
        
        public MonsterGroup Group { get; set; }
        
        private readonly MonsterData _proto;
        private IBehaviour _behaviour;
        private bool _behaviourInitialized;
        private double _deadTime = 5000;
        
        public MonsterEntity(IMonsterManager monsterManager, IAnimationManager animationManager, IWorld world, uint id, int x, int y, float rotation = 0) 
            : base(animationManager, world.GenerateVid())
        {
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

        public async override Task Update(double elapsedTime)
        {
            if (Dead)
            {
                _deadTime -= elapsedTime;
                if (_deadTime <= 0)
                {
                    await Map.DespawnEntity(this);
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

            await base.Update(elapsedTime);
        }

        public override async Task Goto(int x, int y)
        {
            Rotation = (float) MathUtils.Rotation(x - PositionX, y - PositionY);
            
            await base.Goto(x, y);
            
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
            await ForEachNearbyEntity(async entity =>
            {
                if (entity is PlayerEntity player)
                {
                    await player.Connection.Send(movement);
                }
            });
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

        public async override Task<int> Damage(IEntity attacker, EDamageType damageType, int damage)
        {
            damage = await base.Damage(attacker, damageType, damage);

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

        public override ValueTask AddPoint(EPoints point, int value)
        {
            return ValueTask.CompletedTask;
        }

        public override ValueTask SetPoint(EPoints point, uint value)
        {
            return ValueTask.CompletedTask;
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
            Log.Warning($"Point {point} is not implemented on monster");
            return 0;
        }

        public override async ValueTask Die()
        {
            if (Dead)
            {
                return;
            }
            
            await base.Die();

            var dead = new CharacterDead { Vid = Vid };
            await ForEachNearbyEntity(async entity =>
            {
                if (entity is PlayerEntity player)
                {
                    await player.Connection.Send(dead);
                }
            });
        }

        protected override ValueTask OnNewNearbyEntity(IEntity entity)
        {
            _behaviour?.OnNewNearbyEntity(entity);
        
            return ValueTask.CompletedTask;
        }

        protected override ValueTask OnRemoveNearbyEntity(IEntity entity)
        {
            return ValueTask.CompletedTask;
        }

        public override ValueTask OnDespawn()
        {
            if (Group != null)
            {
                Group.Monsters.Remove(this);
                if (Group.Monsters.Count == 0)
                {
                    (Map as Map)?.EnqueueGroupRespawn(Group);
                }
            }
        
            return ValueTask.CompletedTask;
        }

        public override async Task ShowEntity(IConnection connection)
        {
            if (Dead)
            {
                return; // no need to send dead entities to new players
            }
            
            await connection.Send(new SpawnCharacter
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
                await connection.Send(new CharacterInfo {
                    Vid = Vid,
                    Empire = _proto.Empire,
                    Level = _proto.Level,
                    Name = _proto.TranslatedName
                });
            }
        }
        
        public async override Task HideEntity(IConnection connection)
        {
            await connection.Send(new RemoveCharacter
            {
                Vid = Vid
            });
        }

        public override string ToString()
        {
            return $"{_proto.TranslatedName.Trim((char) 0x00)} ({_proto.Id})";
        }
    }
}