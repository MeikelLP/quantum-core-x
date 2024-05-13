using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.API.Game.World.AI;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets;
using QuantumCore.Game.PlayerUtils;
using QuantumCore.Game.Services;
using QuantumCore.Game.World.AI;

namespace QuantumCore.Game.World.Entities
{
    public class MonsterEntity : Entity
    {
        private readonly IDropProvider _dropProvider;
        private readonly ILogger _logger;
        public override EEntityType Type => EEntityType.Monster;
        public bool IsStone => _proto.Type == (byte) EEntityType.MetinStone;
        public EEntityRank Rank => (EEntityRank) _proto.Rank;

        public IBehaviour? Behaviour
        {
            get { return _behaviour; }
            set
            {
                _behaviour = value;
                _behaviourInitialized = false;
            }
        }

        public override byte HealthPercentage
        {
            get { return (byte) (Math.Min(Math.Max(Health / (double) _proto.Hp, 0), 1) * 100); }
        }

        public MonsterData Proto
        {
            get { return _proto; }
        }

        public MonsterGroup? Group { get; set; }

        private readonly MonsterData _proto;
        private IBehaviour? _behaviour;
        private bool _behaviourInitialized;
        private double _deadTime = 5000;
        private readonly IMap _map;
        private readonly IItemManager _itemManager;

        public MonsterEntity(IMonsterManager monsterManager, IDropProvider dropProvider,
            IAnimationManager animationManager,
            IMap map, ILogger logger, IItemManager itemManager, uint id, int x, int y, float rotation = 0)
            : base(animationManager, map.World.GenerateVid())
        {
            var proto = monsterManager.GetMonster(id);

            if (proto is null)
            {
                // todo handle better
                throw new InvalidOperationException($"Could not find mob proto for ID {id}. Cannot create mob entity");
            }

            _map = map;
            _dropProvider = dropProvider;
            _logger = logger;
            _itemManager = itemManager;
            _proto = proto;
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
            else if (_proto.Type == (byte) EEntityType.Npc)
            {
                // npc
            }
        }

        public override void Update(double elapsedTime)
        {
            if (Map is null) return;
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

            if (!Dead)
            {
                _behaviour?.Update(elapsedTime);
            }

            base.Update(elapsedTime);
        }

        public override void Goto(int x, int y)
        {
            Rotation = (float) MathUtils.Rotation(x - PositionX, y - PositionY);

            base.Goto(x, y);
            // Send movement to nearby players
            var movement = new CharacterMoveOut
            {
                Vid = Vid,
                Rotation = (byte) (Rotation / 5),
                Argument = (byte) CharacterMove.CharacterMovementType.Wait,
                PositionX = TargetPositionX,
                PositionY = TargetPositionY,
                Time = (uint) GameServer.Instance.ServerTime,
                Duration = MovementDuration
            };

            foreach (var entity in NearbyEntities)
            {
                if (entity is PlayerEntity player)
                {
                    player.Connection.Send(movement);
                }
            }
        }

        public override byte GetBattleType()
        {
            return _proto.BattleType;
        }

        public override int GetMinDamage()
        {
            return (int) _proto.DamageRange[0];
        }

        public override int GetMaxDamage()
        {
            return (int) _proto.DamageRange[1];
        }

        public override int GetBonusDamage()
        {
            return 0; // monster don't have bonus damage as players have from their weapon
        }

        public override int Damage(IEntity attacker, EDamageType damageType, int damage)
        {
            damage = base.Damage(attacker, damageType, damage);

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

        public override void AddPoint(EPoints point, int value)
        {
        }

        public override void SetPoint(EPoints point, uint value)
        {
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
                    return (uint) (_proto.Level + _proto.Ht + _proto.Defence);
                case EPoints.DefenceBonus:
                    return 0;
                case EPoints.Experience:
                    return _proto.Experience;
            }

            _logger.LogWarning("Point {Point} is not implemented on monster", point);
            return 0;
        }

        public override void Die()
        {
            if (Dead)
            {
                return;
            }

            DoDrops();

            base.Die();

            var dead = new CharacterDead {Vid = Vid};
            foreach (var entity in NearbyEntities)
            {
                if (entity is PlayerEntity player)
                {
                    player.Connection.Send(dead);
                }
            }
        }

        private void DoDrops()
        {
            // no drops if no killer
            if (LastAttacker is null) return;
            
            List<ItemInstance> drops = new();

            bool dropDebug = true; // todo: parse from config
            
            var (delta, range) = _dropProvider.CalculateDropPercentages(LastAttacker, this);
            if (dropDebug)
            {
                _logger.LogInformation("Drop chance for {Name} ({MobProtoId}) is {Delta} with range {Range}", _proto.TranslatedName, _proto.Id, delta, range);
            }
            
            // Common drops (common_drop_item.txt)
            var commonDrops = _dropProvider.GetPossibleCommonDropsForPlayer(LastAttacker);
            foreach (var drop in commonDrops)
            {
                var percent = (drop.Chance * delta) / 100;
                var target = CoreRandom.GenerateInt32(1, range + 1);
                
                if (dropDebug)
                {
                    float realPercent = percent / range * 100;
                    _logger.LogInformation("Drop chance for {Name} ({MobProtoId}) is {RealPercent}%", _proto.TranslatedName, _proto.Id, realPercent);
                }
                
                if (percent >= target)
                {
                    var itemProto = _itemManager.GetItem(drop.ItemProtoId);
                    if (itemProto is null)
                    {
                        _logger.LogWarning("Could not find item proto for {ItemProtoId}", drop.ItemProtoId);
                        continue;
                    }
                    
                    var itemInstance = _itemManager.CreateItem(itemProto);

                    if ((EItemType) itemProto.Type ==  EItemType.Polymorph)
                    {
                        if (Proto.PolymorphItemId == itemProto.Id)
                        {
                            // todo: set item socket 0 to race number (when ItemInstance have sockets implemented)
                        }
                    }
                    
                    drops.Add(itemInstance);
                }
            }
            
            // Drop Item Group (drop_item_group.txt)
            // TODO: so far, was not able to find any example of this file anywhere. We can implement the logic, but without any example file, it's "impossible" to test.

            // Mob Drop Item Group (mob_drop_item.txt)
            var mobDrops = _dropProvider.GetPossibleMobDropsForPlayer(LastAttacker, _proto.Id);
            if (mobDrops.IsDefaultOrEmpty)
            {
                mobDrops = [];
            }
            foreach (var drop in mobDrops)
            {
                var percent = 40000 * delta / drop.MinKillCount;
                var target = CoreRandom.GenerateInt32(1, range + 1);

                if (dropDebug)
                {
                    float realPercent = (float) percent / range * 100;
                    _logger.LogInformation("Drop chance for {Name} ({MobProtoId}) is {RealPercent}%", _proto.TranslatedName, _proto.Id, realPercent);
                }
                
                if (percent > target)
                {
                    var itemProto = _itemManager.GetItem(drop.ItemProtoId);
                    if (itemProto is null)
                    {
                        _logger.LogWarning("Could not find item proto for {ItemProtoId}", drop.ItemProtoId);
                        continue;
                    }
                    
                    var itemInstance = _itemManager.CreateItem(itemProto, drop.Amount);
                    drops.Add(itemInstance);
                }
            }

            // Finally, drop the items
            foreach (var drop in drops)
            {
                _map.AddGroundItem(drop, PositionX, PositionY, 1, LastAttacker.Name);
            }
        }

        protected override void OnNewNearbyEntity(IEntity entity)
        {
            _behaviour?.OnNewNearbyEntity(entity);
        }

        protected override void OnRemoveNearbyEntity(IEntity entity)
        {
        }

        public override void OnDespawn()
        {
            if (Group != null)
            {
                Group.Monsters.Remove(this);
                if (Group.Monsters.Count == 0)
                {
                    (Map as Map)?.EnqueueGroupRespawn(Group);
                }
            }
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
                connection.Send(new CharacterInfo
                {
                    Vid = Vid,
                    Empire = _proto.Empire,
                    Level = 0,
                    Name = _proto.TranslatedName
                });
            }
            // foreach (var drop in _dropProvider.CommonDrops)
            // {
            //     if (!drop.CanDropFor(LastAttacker)) continue;
            //
            //     var chance = drop.Chance * Globals.DROP_MULTIPLIER * Globals.DROP_MULTIPLIER;
            //     if (chance > Random.Shared.NextSingle())
            //     {
            //         var itemInstance = _itemManager.CreateItem(_itemManager.GetItem(drop.ItemProtoId));
            //         _map.AddGroundItem(itemInstance, PositionX, PositionY, Globals.DROP_COMMON_AMOUNT, LastAttacker.Name);
            //     }
            // }
        }

        public override void HideEntity(IConnection connection)
        {
            connection.Send(new RemoveCharacter
            {
                Vid = Vid
            });
        }
        

        public override string ToString()
        {
            return $"{_proto.TranslatedName?.Trim((char) 0x00)} ({_proto.Id})";
        }
    }
}
