using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using QuantumCore.API;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Cache;
using QuantumCore.Core.Networking;
using QuantumCore.Core.Types;
using QuantumCore.Core.Utils;
using QuantumCore.Database;
using QuantumCore.Game.Packets;
using QuantumCore.Game.PlayerUtils;
using Serilog;

namespace QuantumCore.Game.World.Entities
{
    public class PlayerEntity : Entity, IPlayerEntity, IDamageable
    {
        public override EEntityType Type => EEntityType.Player;

        public string Name => Player.Name;
        public GameConnection Connection { get; }
        public Player Player { get; private set; }
        public Inventory Inventory { get; private set; }
        public IEntity Target { get; set; }

        public IList<Guid> Groups { get; private set; }

        public override byte HealthPercentage {
            get {
                return 100; // todo
            }
        }

        public EAntiFlags AntiFlagClass {
            get {
                switch (Player.PlayerClass)
                {
                    case 0:
                    case 4:
                        return EAntiFlags.Warrior;
                    case 1:
                    case 5:
                        return EAntiFlags.Assassin;
                    case 2:
                    case 6:
                        return EAntiFlags.Sura;
                    case 3:
                    case 7:
                        return EAntiFlags.Shaman;
                    default:
                        return 0;
                }
            }
        }

        public EAntiFlags AntiFlagGender {
            get {
                switch (Player.PlayerClass)
                {
                    case 0:
                    case 2:
                    case 5:
                    case 7:
                        return EAntiFlags.Male;
                    case 1:
                    case 3:
                    case 4:
                    case 6:
                        return EAntiFlags.Female;
                    default:
                        return 0;
                }
            }
        }
        
        private byte _attackSpeed = 140;
        private uint _hp = 0;

        private const int _persistInterval = 1000;
        private int _persistTime = 0;

        public PlayerEntity(Player player, GameConnection connection) : base(World.Instance.GenerateVid())
        {
            Connection = connection;
            Player = player;
            PositionX = player.PositionX;
            PositionY = player.PositionY;
            Inventory = new Inventory(player.Id, 1, 5, 9, 2);

            MovementSpeed = 150;
            EntityClass = player.PlayerClass;

            Groups = new List<Guid>();
        }

        public async Task Load()
        {
            await Inventory.Load();
            _hp = GetPoint(EPoints.MaxHp); // todo: cache hp of player 
            await LoadPermGroups();
        }

        private async Task LoadPermGroups()
        {
            var redis = CacheManager.Redis;
            var playerId = Player.Id;

            var playerKey = "perm:" + playerId;
            var list = redis.CreateList<Guid>(playerKey);

            foreach (var group in await list.Range(0, -1))
            {
                Groups.Add(group);
            }
        }

        public override void Move(int x, int y)
        {
            if (PositionX == x && PositionY == y) return;

            World.Instance.DespawnEntity(this);

            PositionX = x;
            PositionY = y;

            // Reset movement info
            Stop();

            // Spawn the player
            if (!World.Instance.SpawnEntity(this))
            {
                Log.Warning("Failed to spawn player entity");
                Connection.Close();
            }

            Show(Connection);

            ForEachNearbyEntity(entity =>
            {
                if (entity is PlayerEntity p)
                {
                    p.Show(Connection);
                }
            });
        }

        public uint GetDefence()
        {
            return 0;
        }

        public long TakeDamage(long damage, Entity attacker)
        {
            _hp -= (uint) damage;

            var damageInfo = new DamageInfo { Vid = Vid, DamageType = 1, Damage = (int) damage };
            Connection.Send(damageInfo);
            
            SendPoints();

            if (_hp <= 0)
            {
                Die();
            }

            return damage;
        }

        public void Attack(IEntity victim)
        {
            if (victim is not IDamageable damageable)
            {
                return;
            }
            
            // Get current equipped weapon
            var weapon = Inventory.EquipmentWindow.Weapon;
            ItemProto.Item weaponItem = null;
            if (weapon != null)
            {
                weaponItem = ItemManager.GetItem(weapon.ItemId);
            }
            
            // For more details on this formula:
            // https://gitlab.com/quantum-core/core-dotnet/-/wikis/Attack-damage-calculation

            // Calculating parameters we need for calculating effective damage base
            var growth = weaponItem != null ? weaponItem.Values[5] : 0;
            var attackBase = weaponItem != null ? CoreRandom.GenerateUInt32((uint)weaponItem.Values[3], (uint)weaponItem.Values[4] + 1) : 0;
            var stat = GetPoint(JobInfo.Get(Player.PlayerClass).AttackStatus);
            var str = GetPoint(EPoints.St);
            var dex = GetPoint(EPoints.Dx);
            var level = GetPoint(EPoints.Level);
            var defence = damageable.GetDefence();
            
            // Calculating effective damage base
            var damageBase = 2 * (level + growth) +
                1 / 75.0 * (70 + (2 * dex - 0.6 * level) / 9.0) * (str + (3 * attackBase + stat) / 2) - defence;
            
            // todo calculate final damage with all the stats
            // todo critical hit?
            // todo chance to miss a hit?

            var actualDamage = damageable.TakeDamage((uint)damageBase, this);
            var info = new DamageInfo {Vid = victim.Vid, DamageType = 1, Damage = (int) actualDamage};
            Connection.Send(info);
        }

        public uint CalculateAttackDamage(uint baseDamage)
        {
            var levelBonus = GetPoint(EPoints.Level) * 2;
            var statusBonus = (
                4 * GetPoint(EPoints.St) +
                2 * GetPoint(JobInfo.Get(Player.PlayerClass).AttackStatus)
            ) / 3;
            var weaponDamage = baseDamage * 2;

            return levelBonus + (statusBonus + weaponDamage) * GetHitRate() / 100;
        }

        public uint GetHitRate()
        {
            var b = (GetPoint(EPoints.Dx) * 4 + GetPoint(EPoints.Level) * 2) / 6;
            return 100 * ((b > 90 ? 90 : b) + 210) / 300;
        }
        
        public override void Update(double elapsedTime)
        {
            if (Map == null) return; // We don't have a map yet so we aren't spawned

            base.Update(elapsedTime);
            
            _persistTime += (int)elapsedTime;
            if (_persistTime > _persistInterval)
            {
                Persist();
                _persistTime -= _persistInterval;
            }
        }

        public uint GetPoint(EPoints point)
        {
            switch (point)
            {
                case EPoints.Level:
                    return Player.Level;
                case EPoints.Hp:
                    return _hp;
                case EPoints.MaxHp:
                    var info = JobInfo.Get(Player.PlayerClass);
                    if (info == null)
                    {
                        return 0;
                    }

                    return info.StartHp + info.HpPerHt * GetPoint(EPoints.Ht) +
                           info.HpPerLevel * GetPoint(EPoints.Level);
                case EPoints.St:
                    return Player.St;
                case EPoints.Ht:
                    return Player.Ht;
                case EPoints.Dx:
                    return Player.Dx;
                case EPoints.Iq:
                    return Player.Iq;
                case EPoints.Gold:
                    return Player.Gold;
                case EPoints.MinWeaponDamage:
                {
                    var weapon = Inventory.EquipmentWindow.Weapon;
                    if (weapon == null)
                    {
                        return 0;
                    }

                    var item = ItemManager.GetItem(weapon.ItemId);
                    return (uint) (item.Values[3] + item.Values[5]);
                }
                case EPoints.MaxWeaponDamage:
                {
                    var weapon = Inventory.EquipmentWindow.Weapon;
                    if (weapon == null)
                    {
                        return 0;
                    }

                    var item = ItemManager.GetItem(weapon.ItemId);
                    return (uint) (item.Values[4] + item.Values[5]);
                }
                case EPoints.MinAttackDamage:
                    return CalculateAttackDamage(GetPoint(EPoints.MinWeaponDamage));
                case EPoints.MaxAttackDamage:
                    return CalculateAttackDamage(GetPoint(EPoints.MaxWeaponDamage));
                default:
                    return 0;
            }
        }

        private async Task Persist()
        {
            Player.PositionX = PositionX;
            Player.PositionY = PositionY;
            
            await CacheManager.Redis.Set($"player:{Player.Id}", Player);
        }

        protected override void OnNewNearbyEntity(IEntity entity)
        {
            entity.ShowEntity(Connection);
        }

        protected override void OnRemoveNearbyEntity(IEntity entity)
        {
            Connection.Send(new RemoveCharacter
            {
                Vid = entity.Vid
            });
        }

        public override void OnDespawn()
        {
            Persist();
        }

        public Item GetItem(byte window, ushort position)
        {
            switch (window)
            {
                case (byte) WindowType.Inventory:
                    if (position >= Inventory.Size)
                    {
                        // Equipment
                        return Inventory.EquipmentWindow.GetItem(position);
                    }
                    else
                    {
                        // Inventory
                        return Inventory.GetItem(position);
                    }
            }

            return null;
        }

        public bool IsSpaceAvailable(Item item, byte window, ushort position)
        {
            switch (window)
            {
                case (byte) WindowType.Inventory:
                    if (position >= Inventory.Size)
                    {
                        // Equipment
                        // Make sure item fits in equipment window
                        if (IsEquippable(item) && Inventory.EquipmentWindow.IsSuitable(item, position))
                        {
                            return Inventory.EquipmentWindow.GetItem(position) == null;
                        }
                        
                        return false;
                    }
                    else
                    {
                        // Inventory
                        return Inventory.IsSpaceAvailable(item, position);
                    }
            }

            return false;
        }

        public bool IsEquippable(Item item)
        {
            var proto = ItemManager.GetItem(item.ItemId);
            if (proto == null)
            {
                // Proto for item not found
                return false;
            }

            if (proto.WearFlags == 0)
            {
                // No wear flags -> not wearable
                return false;
            }
            
            // Check anti flags
            var antiFlags = (EAntiFlags) proto.AntiFlags;
            if (antiFlags.HasFlag(AntiFlagClass))
            {
                return false;
            }

            if (antiFlags.HasFlag(AntiFlagGender))
            {
                return false;
            }

            // Check limits (level)
            foreach (var limit in proto.Limits)
            {
                if (limit.Type == (byte) ELimitType.Level)
                {
                    if (Player.Level < limit.Value)
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }
        
        public void RemoveItem(Item item)
        {
            switch (item.Window)
            {
                case (byte) WindowType.Inventory:
                    if (item.Position >= Inventory.Size)
                    {
                        // Equipment
                        Inventory.EquipmentWindow.RemoveItem(item);
                        SendCharacterUpdate();
                    }
                    else
                    {
                        // Inventory
                        Inventory.RemoveItem(item);
                    }

                    break;
            }
        }

        public async Task SetItem(Item item, byte window, ushort position)
        {
            switch (window)
            {
                case (byte) WindowType.Inventory:
                    if (position >= Inventory.Size)
                    {
                        // Equipment
                        if (Inventory.EquipmentWindow.GetItem(position) == null)
                        {
                            Inventory.EquipmentWindow.SetItem(item, position);
                            await item.Set(Player.Id, window, position);
                            SendCharacterUpdate();
                        }
                    }
                    else
                    {
                        // Inventory
                        await Inventory.PlaceItem(item, position);
                    } 
                    break;
            }
        }

        public override void ShowEntity(IConnection connection)
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
            for (var i = 0; i < points.Points.Length; i++)
            {
                points.Points[i] = GetPoint((EPoints) i);
            }
            Connection.Send(points);
        }

        public void SendInventory()
        {
            foreach (var item in Inventory.Items)
            {
                SendItem(item);
            }

            Inventory.EquipmentWindow.Send(this);
        }

        public void SendItem(Item item)
        {
            Debug.Assert(item.PlayerId == Player.Id);
            
            var p = new SetItem {
                Window = item.Window,
                Position = (ushort)item.Position,
                ItemId = item.ItemId,
                Count = item.Count
            };
            Connection.Send(p);
        }

        public void SendRemoveItem(byte window, ushort position)
        {
            Connection.Send(new SetItem {
                Window = window,
                Position = position,
                ItemId = 0,
                Count = 0
            });
        }

        public void SendCharacter(IConnection connection)
        {
            connection.Send(new SpawnCharacter
            {
                Vid = Vid,
                CharacterType = (byte) EEntityType.Player,
                Angle = 0,
                PositionX = PositionX,
                PositionY = PositionY,
                Class = Player.PlayerClass,
                MoveSpeed = MovementSpeed,
                AttackSpeed = _attackSpeed
            });
        }

        public void SendCharacterAdditional(IConnection connection)
        {
            connection.Send(new CharacterInfo
            {
                Vid = Vid,
                Name = Player.Name,
                Empire = 1, // todo
                Level = Player.Level,
                Parts = new ushort[] {
                    (ushort)(Inventory.EquipmentWindow.Body?.ItemId ?? 0), 
                    (ushort)(Inventory.EquipmentWindow.Weapon?.ItemId ?? 0), 
                    0, 
                    (ushort)(Inventory.EquipmentWindow.Hair?.ItemId ?? 0)
                }
            });
        }

        public void SendCharacterUpdate()
        {
            var packet = new CharacterUpdate {
                Vid = Vid,
                Parts = new ushort[] {
                    (ushort) (Inventory.EquipmentWindow.Body?.ItemId ?? 0),
                    (ushort) (Inventory.EquipmentWindow.Weapon?.ItemId ?? 0), 0,
                    (ushort) (Inventory.EquipmentWindow.Hair?.ItemId ?? 0)
                },
                MoveSpeed = MovementSpeed,
                AttackSpeed = _attackSpeed
            };
            
            Connection.Send(packet);
            
            ForEachNearbyEntity(entity =>
            {
                if (entity is PlayerEntity p)
                {
                    p.Connection.Send(packet);
                }
            });
        }

        public void SendChatMessage(string message)
        {
            var chat = new ChatOutcoming
            {
                MessageType = ChatMessageTypes.Normal,
                Vid = Vid,
                Empire = 1,
                Message = message
            };
            Connection.Send(chat);
        }
		
        public void SendChatCommand(string message)
        {
            var chat = new ChatOutcoming
            {
                MessageType = ChatMessageTypes.Command,
                Vid = 0,
                Empire = 1,
                Message = message
            };
            Connection.Send(chat);
        }
		
        public void SendChatInfo(string message)
        {
            var chat = new ChatOutcoming
            {
                MessageType = ChatMessageTypes.Info,
                Vid = 0,
                Empire = 1,
                Message = message
            };
            Connection.Send(chat);
        }

        public void SendTarget()
        {
            var packet = new SetTarget();
            if (Target != null)
            {
                packet.TargetVid = Target.Vid;
                packet.Percentage = Target.HealthPercentage;
            }
            Connection.Send(packet);
        }
        
        public void Show(Connection connection)
        {
            SendCharacter(connection);
            SendCharacterAdditional(connection);
        }

        public void Disconnect()
        {
            Connection.Close();
        }

        public override string ToString()
        {
            return Player.Name + "(Player)";
        }
    }
}