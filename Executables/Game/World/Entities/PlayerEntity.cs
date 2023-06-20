using System.Data;
using System.Data;
using System.Diagnostics;
using Dapper;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Cache;
using QuantumCore.Core.Utils;
using QuantumCore.Database;
using QuantumCore.Extensions;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets;
using QuantumCore.Game.PlayerUtils;

namespace QuantumCore.Game.World.Entities
{
    public class PlayerEntity : Entity, IPlayerEntity
    {
        public override EEntityType Type => EEntityType.Player;

        public string Name => Player.Name;
        public IGameConnection Connection { get; }
        public PlayerData Player { get; private set; }
        public byte Empire { get; private set; }
        public IInventory Inventory { get; private set; }
        public IEntity Target { get; set; }
        public IList<Guid> Groups { get; private set; }
        public IShop Shop { get; set; }
        public IQuickSlotBar QuickSlotBar { get; }
        public IQuest CurrentQuest { get; set; }
        public Dictionary<string, IQuest> Quests { get; } = new();

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
        private uint _defence;

        private const int PersistInterval = 1000;
        private int _persistTime = 0;
        private const int HealthRegenInterval = 3 * 1000;
        private const int ManaRegenInterval = 3 * 1000;
        private double _healthRegenTime = HealthRegenInterval;
        private double _manaRegenTime = ManaRegenInterval;
        private readonly IItemManager _itemManager;
        private readonly IJobManager _jobManager;
        private readonly IExperienceManager _experienceManager;
        private readonly IDbConnection _db;
        private readonly IQuestManager _questManager;
        private readonly ICacheManager _cacheManager;
        private readonly IWorld _world;
        private readonly ILogger<PlayerEntity> _logger;

        public PlayerEntity(Player player, IGameConnection connection, IItemManager itemManager, IJobManager jobManager,
            IExperienceManager experienceManager, IAnimationManager animationManager, IDbConnection db,
            IQuestManager questManager, ICacheManager cacheManager, IWorld world, ILogger<PlayerEntity> logger) 
            : base(animationManager, world.GenerateVid())
        {
            Connection = connection;
            _itemManager = itemManager;
            _jobManager = jobManager;
            _experienceManager = experienceManager;
            _db = db;
            _questManager = questManager;
            _cacheManager = cacheManager;
            _world = world;
            _logger = logger;
            Player = new PlayerData {
                Id = player.Id,
                AccountId = player.AccountId,
                Name = player.Name,
                PlayerClass = player.PlayerClass,
                SkillGroup = player.SkillGroup,
                PlayTime = player.PlayTime,
                Level = player.Level,
                Experience = player.Experience,
                Gold = player.Gold,
                St = player.St,
                Ht = player.Ht,
                Dx = player.Dx,
                Iq = player.Iq,
                PositionX = player.PositionX,
                PositionY = player.PositionY,
                Health = player.Health,
                Mana = player.Mana,
                Stamina = player.Stamina,
                BodyPart = player.BodyPart,
                HairPart = player.HairPart,
                GivenStatusPoints = player.GivenStatusPoints,
                AvailableStatusPoints = player.AvailableStatusPoints,
            };
            PositionX = player.PositionX;
            PositionY = player.PositionY;
            Inventory = new Inventory(itemManager, db, _cacheManager, _logger, player.Id, 1, 5, 9, 2);
            QuickSlotBar = new QuickSlotBar(_cacheManager, _logger, this);

            MovementSpeed = 150;
            EntityClass = player.PlayerClass;

            Groups = new List<Guid>();
        }

        public async Task Load()
        {
            Empire = await _db.QueryFirstOrDefaultAsync<byte>(
                "SELECT Empire FROM account.accounts WHERE Id = @AccountId", new {AccountId = Player.AccountId});
            await Inventory.Load();
            await QuickSlotBar.Load();
            Health = (int) GetPoint(EPoints.MaxHp); // todo: cache hp of player 
            Mana = (int) GetPoint(EPoints.MaxSp);
            await LoadPermGroups();
            
            _questManager.InitializePlayer(this);
            
            CalculateDefence();
        }

        private async Task LoadPermGroups()
        {
            var playerId = Player.Id;

            var playerKey = "perm:" + playerId;
            var list = _cacheManager.CreateList<Guid>(playerKey);

            foreach (var group in await list.Range(0, -1))
            {
                Groups.Add(group);
            }
        }

        public T GetQuestInstance<T>() where T : IQuest
        {
            var id = typeof(T).FullName;
            if (id == null)
            {
                return default;
            }

            return (T) Quests[id];
        }

        private async Task Warp(int x, int y)
        {
            await _world.DespawnEntity(this);
            
            PositionX = x;
            PositionY = y;
            
            var host = _world.GetMapHost(PositionX, PositionY);

            await Persist();
            _logger.LogInformation("Warp!");
            var packet = new Warp {
                PositionX = PositionX,
                PositionY = PositionY,
                ServerAddress = IpUtils.ConvertIpToUInt(host.Ip),
                ServerPort = host.Port
            };
            await Connection.Send(packet);
        }

        public override async Task Move(int x, int y)
        {
            if (PositionX == x && PositionY == y) return;
            
            if (!Map.IsPositionInside(x, y))
            {
                await Warp(x, y);
                return;
            }

            await _world.DespawnEntity(this);

            PositionX = x;
            PositionY = y;

            // Reset movement info
            Stop();

            // Spawn the player
            if (!await _world.SpawnEntity(this))
            {
                _logger.LogWarning("Failed to spawn player entity");
                Connection.Close();
            }

            await Show(Connection);

            await ForEachNearbyEntity(async entity =>
            {
                if (entity is PlayerEntity p)
                {
                    await p.Show(Connection);
                }
            });
        }

        private void CalculateDefence()
        {
            _defence = GetPoint(EPoints.Level) + (uint)Math.Floor(0.8 * GetPoint(EPoints.Ht));
            
            foreach (var slot in Enum.GetValues<EquipmentSlots>())
            {
                var item = Inventory.EquipmentWindow.GetItem(slot);
                if (item == null) continue;
                var proto = _itemManager.GetItem(item.ItemId);
                if (proto.Type != (byte) EItemType.Armor) continue;

                _defence += (uint)proto.Values[1] + (uint)proto.Values[5] * 2;
            }
            
            _logger.LogDebug("Calculate defence value for {Name}, result: {Defence}", Name, _defence);
            
            // todo add defence bonus from quests
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
            await Connection.Send(dead);
        }

        public async Task Respawn(bool town)
        {
            if (!Dead)
            {
                return;
            }

            Shop?.Close(this);

            Dead = false;
            
            // todo implement respawn in town
            // todo spawn with invisible affect
            
            await SendChatCommand("CloseRestartWindow");
            await Connection.SetPhaseAsync(EPhases.Game);

            var remove = new RemoveCharacter { Vid = Vid };
            
            await Connection.Send(remove);
            await Show(Connection);
            
            await ForEachNearbyEntity(async entity =>
            {
                if (entity is PlayerEntity pe)
                {
                    await ShowEntity(pe.Connection);
                }
                
                await entity.ShowEntity(Connection);
            });

            Health = 50;
            Mana = 50;
            await SendPoints();
        }

        private void GiveStatusPoints()
        {
            var shouldHavePoints = (uint) ((Player.Level - 1) * 3);
            var steps = (byte) Math.Floor(GetPoint(EPoints.Experience) / (double)GetPoint(EPoints.NeededExperience) * 4);
            shouldHavePoints += steps;
            
            if (shouldHavePoints <= Player.GivenStatusPoints)
            {
                // Remove available points if possible
                var tooMuch = Player.GivenStatusPoints - shouldHavePoints;
                if (Player.AvailableStatusPoints < tooMuch)
                {
                    tooMuch = Player.AvailableStatusPoints;
                }

                Player.AvailableStatusPoints -= tooMuch;
                Player.GivenStatusPoints -= tooMuch;

                return;
            }
            
            Player.AvailableStatusPoints += shouldHavePoints - Player.GivenStatusPoints;
            Player.GivenStatusPoints = shouldHavePoints;
        }

        private async ValueTask<bool> CheckLevelUp()
        {
            var exp = GetPoint(EPoints.Experience);
            var needed = GetPoint(EPoints.NeededExperience);
            
            if (exp >= needed)
            {
                // todo level up animation
                
                await AddPoint(EPoints.Level, 1);
                await SetPoint(EPoints.Experience, exp - needed);

                if (!await CheckLevelUp())
                {
                    await SendPoints();
                }

                return true;
            }
            
            GiveStatusPoints();
            return false;
        }

        public uint CalculateAttackDamage(uint baseDamage)
        {
            var levelBonus = GetPoint(EPoints.Level) * 2;
            var statusBonus = (
                4 * GetPoint(EPoints.St) +
                2 * GetPoint(_jobManager.Get(Player.PlayerClass).AttackStatus)
            ) / 3;
            var weaponDamage = baseDamage * 2;

            return levelBonus + (statusBonus + weaponDamage) * GetHitRate() / 100;
        }

        public uint GetHitRate()
        {
            var b = (GetPoint(EPoints.Dx) * 4 + GetPoint(EPoints.Level) * 2) / 6;
            return 100 * ((b > 90 ? 90 : b) + 210) / 300;
        }
        
        public async override Task Update(double elapsedTime)
        {
            if (Map == null) return; // We don't have a map yet so we aren't spawned

            await base.Update(elapsedTime);

            var maxHp = GetPoint(EPoints.MaxHp);
            if (Health < maxHp)
            {
                _healthRegenTime -= elapsedTime;
                if (_healthRegenTime <= 0)
                {
                    var factor = State == EEntityState.Idle ? 0.05 : 0.01;
                    Health = Math.Min((int) maxHp, Health + 15 + (int) (maxHp * factor));
                    await SendPoints();

                    _healthRegenTime += HealthRegenInterval;
                }
            }
            var maxSp = GetPoint(EPoints.MaxSp);
            if (Mana < maxSp)
            {
                _manaRegenTime -= elapsedTime;
                if (_manaRegenTime <= 0)
                {
                    var factor = State == EEntityState.Idle ? 0.05 : 0.01;
                    Mana = Math.Min((int) maxSp, Mana + 15 + (int) (maxSp * factor));
                    await SendPoints();

                    _manaRegenTime += ManaRegenInterval;
                }
            }

            _persistTime += (int)elapsedTime;
            if (_persistTime > PersistInterval)
            {
                await Persist();
                _persistTime -= PersistInterval;
            }
        }

        public override byte GetBattleType()
        {
            return 0;
        }

        public override int GetMinDamage()
        {
            var weapon = Inventory.EquipmentWindow.Weapon;
            if (weapon == null) return 0;
            var item = _itemManager.GetItem(weapon.ItemId);
            if (item == null) return 0;
            return item.Values[3];
        }

        public override int GetMaxDamage()
        {
            var weapon = Inventory.EquipmentWindow.Weapon;
            if (weapon == null) return 0;
            var item = _itemManager.GetItem(weapon.ItemId);
            if (item == null) return 0;
            return item.Values[4];
        }

        public override int GetBonusDamage()
        {
            var weapon = Inventory.EquipmentWindow.Weapon;
            if (weapon == null) return 0;
            var item = _itemManager.GetItem(weapon.ItemId);
            if (item == null) return 0;
            return item.Values[5];
        }

        public override async ValueTask AddPoint(EPoints point, int value)
        {
            if (value == 0)
            {
                return;
            }
            
            switch (point)
            {
                case EPoints.Level:
                    Player.Level = (byte)(Player.Level + value);
                    await ForEachNearbyEntity(async entity =>
                    {
                        if (entity is IPlayerEntity other)
                        {
                            await SendCharacterAdditional(other.Connection);
                        }
                    });
                    GiveStatusPoints();
                    break;
                case EPoints.Experience:
                    if (_experienceManager.GetNeededExperience((byte)GetPoint(EPoints.Level)) == 0)
                    {
                        // we cannot add experience if no level up is possible
                        return;
                    }
                    if (value < 0 && Player.Experience <= -value)
                    {
                        Player.Experience = 0;
                    }
                    else
                    {
                        Player.Experience = (uint) (Player.Experience + value);
                    }

                    if (value > 0)
                    {
                        await CheckLevelUp();
                    }
                    
                    break;
                case EPoints.Gold:
                    var gold = Player.Gold + value;
                    Player.Gold = (uint) Math.Min(uint.MaxValue, Math.Max(0, gold));
                    break;
                case EPoints.St:
                    Player.St += (byte) value;
                    break;
                case EPoints.Dx:
                    Player.Dx += (byte) value;
                    break;
                case EPoints.Ht:
                    Player.Ht += (byte) value;
                    break;
                case EPoints.Iq:
                    Player.Iq += (byte) value;
                    break;
                case EPoints.Hp:
                    if (value <= 0)
                    {
                        // 0 gets ignored by client
                        // Setting the Hp to 0 does not register as killing the player
                    }
                    else if (value > GetPoint(EPoints.MaxHp))
                    {
                        Health = GetPoint(EPoints.MaxHp);
                    }
                    else
                    {
                        Health = value;
                    }

                    break;
                case EPoints.Sp:
                    if (value <= 0)
                    {
                        // 0 gets ignored by client
                    }
                    else if (value > GetPoint(EPoints.MaxSp))
                    {
                        Mana = GetPoint(EPoints.MaxSp);
                    }
                    else
                    {
                        Mana = value;
                    }

                    break;
                case EPoints.StatusPoints:
                    Player.AvailableStatusPoints += (uint) value;
                    break;
                default:
                    _logger.LogError("Failed to add point to {Point}, unsupported", point);
                    break;
            }
        }

        public override async ValueTask SetPoint(EPoints point, uint value)
        {
            switch (point)
            {
                case EPoints.Level:
                    Player.Level = (byte) value;
                    await ForEachNearbyEntity(async entity =>
                    {
                        if (entity is IPlayerEntity other)
                        {
                            await SendCharacterAdditional(other.Connection);
                        }
                    });
                    GiveStatusPoints();
                    break;
                case EPoints.Experience:
                    Player.Experience = value;
                    await CheckLevelUp();
                    break;
                case EPoints.Gold:
                    Player.Gold = value;
                    break;
                default:
                    _logger.LogError("Failed to set point to {Point}, unsupported", point);
                    break;
            }
        }

        public override uint GetPoint(EPoints point)
        {
            switch (point)
            {
                case EPoints.Level:
                    return Player.Level;
                case EPoints.Experience:
                    return Player.Experience;
                case EPoints.NeededExperience:
                    return _experienceManager.GetNeededExperience(Player.Level);
                case EPoints.Hp:
                    return (uint) Health;
                case EPoints.Sp:
                    return (uint) Mana;
                case EPoints.MaxHp:
                    var info = _jobManager.Get(Player.PlayerClass);
                    if (info == null)
                    {
                        _logger.LogWarning("Job not found: {Job}" , Player.PlayerClass);
                        return 0;
                    }

                    return info.StartHp + info.HpPerHt * GetPoint(EPoints.Ht) +
                           info.HpPerLevel * GetPoint(EPoints.Level);
                case EPoints.MaxSp:
                    info = _jobManager.Get(Player.PlayerClass);
                    if (info == null)
                    {
                        _logger.LogWarning("Job not found: {Job}", Player.PlayerClass);
                        return 0;
                    }
                    return info.StartSp + info.SpPerIq * GetPoint(EPoints.Iq) +
                           info.SpPerLevel * GetPoint(EPoints.Level);
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

                    var item = _itemManager.GetItem(weapon.ItemId);
                    return (uint) (item.Values[3] + item.Values[5]);
                }
                case EPoints.MaxWeaponDamage:
                {
                    var weapon = Inventory.EquipmentWindow.Weapon;
                    if (weapon == null)
                    {
                        return 0;
                    }

                    var item = _itemManager.GetItem(weapon.ItemId);
                    return (uint) (item.Values[4] + item.Values[5]);
                }
                case EPoints.MinAttackDamage:
                    return CalculateAttackDamage(GetPoint(EPoints.MinWeaponDamage));
                case EPoints.MaxAttackDamage:
                    return CalculateAttackDamage(GetPoint(EPoints.MaxWeaponDamage));
                case EPoints.Defence:
                case EPoints.DefenceGrade:
                    return _defence;
                case EPoints.StatusPoints:
                    return Player.AvailableStatusPoints;
                default:
                    if (Enum.GetValues<EPoints>().Contains(point))
                    {
                        _logger.LogWarning("Point {Point} is not implemented on player", point);
                    }

                    return 0;
            }
        }

        private async Task Persist()
        {
            await QuickSlotBar.Persist();
            
            Player.PositionX = PositionX;
            Player.PositionY = PositionY;
            
            await _cacheManager.Set($"player:{Player.Id}", Player);
        }

        protected override async ValueTask OnNewNearbyEntity(IEntity entity)
        {
            await entity.ShowEntity(Connection);
        }

        protected override async ValueTask OnRemoveNearbyEntity(IEntity entity)
        {
            await entity.HideEntity(Connection);
        }

        public async Task DropItem(ItemInstance item, byte count)
        {
            if (count > item.Count)
            {
                return;
            }

            if (item.Count == count)
            {
                await RemoveItem(item);
                await SendRemoveItem(item.Window, (ushort) item.Position);
                await item.Set(_cacheManager, Guid.Empty, 0, 0);
            }
            else
            {
                item.Count -= count;
                await item.Persist(_cacheManager);
                
                await SendItem(item);

                item = _itemManager.CreateItem(_itemManager.GetItem(item.ItemId), count);
            }

            (Map as Map)?.AddGroundItem(item, PositionX, PositionY);
        }

        public async Task Pickup(IGroundItem groundItem)
        {
            var item = groundItem.Item;
            if (item.ItemId == 1)
            {
                await AddPoint(EPoints.Gold, (int) groundItem.Amount);
                await SendPoints();
                await Map.DespawnEntity(groundItem);

                return;
            }

            if (!await Inventory.PlaceItem(item))
            {
                await SendChatInfo("No inventory space left");
                return;
            }
            
            await SendItem(item);
            await Map.DespawnEntity(groundItem);
        }

        public async Task DropGold(uint amount)
        {
            // todo prevent crashing the server with dropping gold too often ;)
            
            if (amount > GetPoint(EPoints.Gold))
            {
                return; // We can't drop more gold than we have ^^
            }
            
            await AddPoint(EPoints.Gold, -(int)amount);
            await SendPoints();

            var item = _itemManager.CreateItem(_itemManager.GetItem(1), 1); // count will be overwritten as it's gold
            (Map as Map)?.AddGroundItem(item, PositionX, PositionY, amount); // todo add method to IMap interface when we have an item interface...
        }

        public async override ValueTask OnDespawn()
        {
            await Persist();
        }

        public ItemInstance GetItem(byte window, ushort position)
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

        public bool IsSpaceAvailable(ItemInstance item, byte window, ushort position)
        {
            switch (window)
            {
                case (byte) WindowType.Inventory:
                    if (position >= Inventory.Size)
                    {
                        // Equipment
                        // Make sure item fits in equipment window
                        if (IsEquippable(item) && Inventory.EquipmentWindow.IsSuitable(_itemManager, item, position))
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

        public bool IsEquippable(ItemInstance item)
        {
            var proto = _itemManager.GetItem(item.ItemId);
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

        public async Task<bool> DestroyItem(ItemInstance item)
        {
            await RemoveItem(item);
            if (!await item.Destroy(_cacheManager))
            {
                return false;
            }

            await SendRemoveItem(item.Window, (ushort) item.Position);
            return true;
        }
        
        public async Task RemoveItem(ItemInstance item)
        {
            switch (item.Window)
            {
                case (byte) WindowType.Inventory:
                    if (item.Position >= Inventory.Size)
                    {
                        // Equipment
                        Inventory.EquipmentWindow.RemoveItem(item);
                        CalculateDefence();
                        await SendCharacterUpdate();
                        await SendPoints();
                    }
                    else
                    {
                        // Inventory
                        Inventory.RemoveItem(item);
                    }

                    break;
            }
        }

        public async Task SetItem(ItemInstance item, byte window, ushort position)
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
                            await item.Set(_cacheManager, Player.Id, window, position);
                            CalculateDefence();
                            await SendCharacterUpdate();
                            await SendPoints();
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

        public override async Task ShowEntity(IConnection connection)
        {
            await SendCharacter(connection);
            await SendCharacterAdditional(connection);
        }

        public override async Task HideEntity(IConnection connection)
        {
            await connection.Send(new RemoveCharacter
            {
                Vid = Vid
            });
        }

        public async Task SendBasicData()
        {
            var details = new CharacterDetails
            {
                Vid = Vid,
                Name = Player.Name,
                Class = Player.PlayerClass,
                PositionX = PositionX,
                PositionY = PositionY,
                Empire = Empire
            };
            await Connection.Send(details);
        }

        public async Task SendPoints()
        {
            var points = new CharacterPoints();
            for (var i = 0; i < points.Points.Length; i++)
            {
                points.Points[i] = GetPoint((EPoints) i);
            }
            await Connection.Send(points);
        }

        public async Task SendInventory()
        {
            foreach (var item in Inventory.Items)
            {
                await SendItem(item);
            }

            await Inventory.EquipmentWindow.Send(this);
        }

        public async Task SendItem(ItemInstance item)
        {
            Debug.Assert(item.PlayerId == Player.Id);
            
            var p = new SetItem {
                Window = item.Window,
                Position = (ushort)item.Position,
                ItemId = item.ItemId,
                Count = item.Count
            };
            await Connection.Send(p);
        }

        public async Task SendRemoveItem(byte window, ushort position)
        {
            await Connection.Send(new SetItem {
                Window = window,
                Position = position,
                ItemId = 0,
                Count = 0
            });
        }

        public async Task SendCharacter(IConnection connection)
        {
            await connection.Send(new SpawnCharacter
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

        public async Task SendCharacterAdditional(IConnection connection)
        {
            await connection.Send(new CharacterInfo
            {
                Vid = Vid,
                Name = Player.Name,
                Empire = Empire,
                Level = Player.Level,
                Parts = new ushort[] {
                    (ushort)(Inventory.EquipmentWindow.Body?.ItemId ?? 0), 
                    (ushort)(Inventory.EquipmentWindow.Weapon?.ItemId ?? 0), 
                    0, 
                    (ushort)(Inventory.EquipmentWindow.Hair?.ItemId ?? 0)
                }
            });
        }

        public async Task SendCharacterUpdate()
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
            
            await Connection.Send(packet);
            
            await ForEachNearbyEntity(async entity =>
            {
                if (entity is PlayerEntity p)
                {
                    await p.Connection.Send(packet);
                }
            });
        }

        public async Task SendChatMessage(string message)
        {
            var chat = new ChatOutcoming
            {
                MessageType = ChatMessageTypes.Normal,
                Vid = Vid,
                Empire = Empire,
                Message = message
            };
            await Connection.Send(chat);
        }
		
        public async Task SendChatCommand(string message)
        {
            var chat = new ChatOutcoming
            {
                MessageType = ChatMessageTypes.Command,
                Vid = 0,
                Empire = Empire,
                Message = message
            };
            await Connection.Send(chat);
        }
		
        public async Task SendChatInfo(string message)
        {
            var chat = new ChatOutcoming
            {
                MessageType = ChatMessageTypes.Info,
                Vid = 0,
                Empire = Empire,
                Message = message
            };
            await Connection.Send(chat);
        }

        public async Task SendTarget()
        {
            var packet = new SetTarget();
            if (Target != null)
            {
                packet.TargetVid = Target.Vid;
                packet.Percentage = Target.HealthPercentage;
            }
            await Connection.Send(packet);
        }
        
        public async Task Show(IConnection connection)
        {
            await SendCharacter(connection);
            await SendCharacterAdditional(connection);
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