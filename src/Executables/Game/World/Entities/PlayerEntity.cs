using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Caching;
using QuantumCore.Core.Utils;
using QuantumCore.Extensions;
using QuantumCore.Game.Packets;
using QuantumCore.Game.Persistence;
using QuantumCore.Game.PlayerUtils;

namespace QuantumCore.Game.World.Entities
{
    public class PlayerEntity : Entity, IPlayerEntity, IDisposable
    {
        public override EEntityType Type => EEntityType.Player;

        public string Name => Player.Name;
        public IGameConnection Connection { get; }
        public PlayerData Player { get; private set; }
        public IInventory Inventory { get; private set; }
        public IEntity? Target { get; set; }
        public IList<Guid> Groups { get; private set; }
        public IShop? Shop { get; set; }
        public IQuickSlotBar QuickSlotBar { get; }
        public IQuest? CurrentQuest { get; set; }
        public Dictionary<string, IQuest> Quests { get; } = new();

        public override byte HealthPercentage
        {
            get
            {
                return 100; // todo
            }
        }

        public EAntiFlags AntiFlagClass
        {
            get
            {
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

        public EAntiFlags AntiFlagGender
        {
            get
            {
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

        private const int PersistInterval = 30 * 1000; // 30s
        private int _persistTime = 0;
        private const int HealthRegenInterval = 3 * 1000;
        private const int ManaRegenInterval = 3 * 1000;
        private double _healthRegenTime = HealthRegenInterval;
        private double _manaRegenTime = ManaRegenInterval;
        private readonly IItemManager _itemManager;
        private readonly IJobManager _jobManager;
        private readonly IExperienceManager _experienceManager;
        private readonly IQuestManager _questManager;
        private readonly ICacheManager _cacheManager;
        private readonly IWorld _world;
        private readonly ILogger<PlayerEntity> _logger;
        private readonly IServiceScope _scope;

        public PlayerEntity(PlayerData player, IGameConnection connection, IItemManager itemManager,
            IJobManager jobManager,
            IExperienceManager experienceManager, IAnimationManager animationManager,
            IQuestManager questManager, ICacheManager cacheManager, IWorld world, ILogger<PlayerEntity> logger,
            IServiceProvider serviceProvider)
            : base(animationManager, world.GenerateVid())
        {
            Connection = connection;
            _itemManager = itemManager;
            _jobManager = jobManager;
            _experienceManager = experienceManager;
            _questManager = questManager;
            _cacheManager = cacheManager;
            _world = world;
            _logger = logger;
            _scope = serviceProvider.CreateScope();
            var itemRepository = _scope.ServiceProvider.GetRequiredService<IItemRepository>();
            Inventory = new Inventory(itemManager, _cacheManager, _logger, itemRepository, player.Id,
                (byte) WindowType.Inventory, InventoryConstants.DEFAULT_INVENTORY_WIDTH,
                InventoryConstants.DEFAULT_INVENTORY_HEIGHT, InventoryConstants.DEFAULT_INVENTORY_PAGES);
            Inventory.OnSlotChanged += Inventory_OnSlotChanged;
            Player = player;
            Empire = player.Empire;
            PositionX = player.PositionX;
            PositionY = player.PositionY;
            QuickSlotBar = new QuickSlotBar(_cacheManager, _logger, this);

            MovementSpeed = 150;
            EntityClass = player.PlayerClass;

            Groups = new List<Guid>();
        }

        private static uint GetMaxSp(IJobManager jobManager, byte playerClass, byte level, uint point)
        {
            var info = jobManager.Get(playerClass);
            if (info == null)
            {
                return 0;
            }

            return info.StartSp + info.SpPerIq * point + info.SpPerLevel * level;
        }

        private static uint GetMaxHp(IJobManager jobManager, byte playerClass, byte level, uint point)
        {
            var info = jobManager.Get(playerClass);
            if (info == null)
            {
                return 0;
            }

            return info.StartHp + info.HpPerHt * point + info.HpPerLevel * level;
        }

        public async Task Load()
        {
            await Inventory.Load();
            await QuickSlotBar.Load();
            Player.MaxHp = GetMaxHp(_jobManager, Player.PlayerClass, Player.Level, Player.Ht);
            Player.MaxSp = GetMaxSp(_jobManager, Player.PlayerClass, Player.Level, Player.Iq);
            Health = (int) GetPoint(EPoints.MaxHp); // todo: cache hp of player
            Mana = (int) GetPoint(EPoints.MaxSp);
            await LoadPermGroups();
            _questManager.InitializePlayer(this);

            CalculateDefence();
        }

        public async Task ReloadPermissions()
        {
            Groups.Clear();
            await LoadPermGroups();
        }

        private async Task LoadPermGroups()
        {
            var commandPermissionRepository = _scope.ServiceProvider.GetRequiredService<ICommandPermissionRepository>();
            var playerId = Player.Id;

            var groups = await commandPermissionRepository.GetGroupsForPlayer(playerId);

            foreach (var group in groups)
            {
                Groups.Add(group);
            }
        }

        public T? GetQuestInstance<T>() where T : class, IQuest
        {
            var id = typeof(T).FullName;
            if (id == null)
            {
                return default;
            }

            return (T) Quests[id];
        }

        private void Warp(int x, int y)
        {
            _world.DespawnEntity(this);

            PositionX = x;
            PositionY = y;

            var host = _world.GetMapHost(PositionX, PositionY);

            _logger.LogInformation("Warp!");
            var packet = new Warp
            {
                PositionX = PositionX,
                PositionY = PositionY,
                ServerAddress = IpUtils.ConvertIpToUInt(host.Ip),
                ServerPort = host.Port
            };
            Connection.Send(packet);
        }

        public override void Move(int x, int y)
        {
            if (Map is null) return;
            if (PositionX == x && PositionY == y) return;

            if (!Map.IsPositionInside(x, y))
            {
                Warp(x, y);
                return;
            }

            PositionX = x;
            PositionY = y;

            // Reset movement info
            Stop();
        }

        private void CalculateDefence()
        {
            _defence = GetPoint(EPoints.Level) + (uint) Math.Floor(0.8 * GetPoint(EPoints.Ht));

            foreach (var slot in Enum.GetValues<EquipmentSlots>())
            {
                var item = Inventory.EquipmentWindow.GetItem(slot);
                if (item == null) continue;
                var proto = _itemManager.GetItem(item.ItemId);
                if (proto?.Type != (byte) EItemType.Armor) continue;

                _defence += (uint) proto.Values[1] + (uint) proto.Values[5] * 2;
            }

            _logger.LogDebug("Calculate defence value for {Name}, result: {Defence}", Name, _defence);

            // todo add defence bonus from quests
        }

        public override void Die()
        {
            if (Dead)
            {
                return;
            }

            base.Die();

            var dead = new CharacterDead {Vid = Vid};
            foreach (var entity in NearbyEntities)
            {
                if (entity is PlayerEntity player)
                {
                    player.Connection.Send(dead);
                }
            }

            Connection.Send(dead);
        }

        public void Respawn(bool town)
        {
            if (!Dead)
            {
                return;
            }

            Shop?.Close(this);

            Dead = false;

            // todo implement respawn in town
            // todo spawn with invisible affect

            SendChatCommand("CloseRestartWindow");
            Connection.SetPhase(EPhases.Game);

            var remove = new RemoveCharacter {Vid = Vid};

            Connection.Send(remove);
            ShowEntity(Connection);

            foreach (var entity in NearbyEntities)
            {
                if (entity is PlayerEntity pe)
                {
                    ShowEntity(pe.Connection);
                }

                entity.ShowEntity(Connection);
            }

            Health = 50;
            Mana = 50;
            SendPoints();
        }

        private void GiveStatusPoints()
        {
            var shouldHavePoints = (uint) ((Player.Level - 1) * 3);
            var steps = (byte) Math.Floor(
                GetPoint(EPoints.Experience) / (double) GetPoint(EPoints.NeededExperience) * 4);
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

        private bool CheckLevelUp()
        {
            var exp = GetPoint(EPoints.Experience);
            var needed = GetPoint(EPoints.NeededExperience);

            if (exp >= needed)
            {
                SetPoint(EPoints.Experience, exp - needed);
                LevelUp();

                if (!CheckLevelUp())
                {
                    SendPoints();
                }

                return true;
            }

            GiveStatusPoints();
            return false;
        }

        private void LevelUp(int level = 1)
        {
            if (Player.Level + level > _experienceManager.MaxLevel)
            {
                return;
            }
            
            AddPoint(EPoints.Skill, level);
            AddPoint(EPoints.SubSkill, level < 10 ? 0 : level - Math.Max((int)Player.Level, 9));
            
            Player.Level = (byte) (Player.Level + level);
            
            // todo: animation
            
            foreach (var entity in NearbyEntities)
            {
                if (entity is IPlayerEntity other)
                {
                    SendCharacterAdditional(other.Connection);
                }
            }

            GiveStatusPoints();
            SendPoints();
        }

        public uint CalculateAttackDamage(uint baseDamage)
        {
            var attackStatus = _jobManager.Get(Player.PlayerClass)?.AttackStatus;

            if (attackStatus is null) return 0;

            var levelBonus = GetPoint(EPoints.Level) * 2;
            var statusBonus = (
                4 * GetPoint(EPoints.St) +
                2 * GetPoint(attackStatus.Value)
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

            var maxHp = GetPoint(EPoints.MaxHp);
            if (Health < maxHp)
            {
                _healthRegenTime -= elapsedTime;
                if (_healthRegenTime <= 0)
                {
                    var factor = State == EEntityState.Idle ? 0.05 : 0.01;
                    Health = Math.Min((int) maxHp, Health + 15 + (int) (maxHp * factor));
                    SendPoints();

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
                    SendPoints();

                    _manaRegenTime += ManaRegenInterval;
                }
            }

            _persistTime += (int) elapsedTime;
            if (_persistTime > PersistInterval)
            {
                Persist().Wait(); // TODO
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

        public override void AddPoint(EPoints point, int value)
        {
            if (value == 0)
            {
                return;
            }

            switch (point)
            {
                case EPoints.Level:
                    LevelUp(value);
                    break;
                case EPoints.Experience:
                    if (_experienceManager.GetNeededExperience((byte) GetPoint(EPoints.Level)) == 0)
                    {
                        // we cannot add experience if no level up is possible
                        return;
                    }

                    var before = Player.Experience;
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
                        var partialLevelUps = CalcPartialLevelUps(before, GetPoint(EPoints.Experience),
                            GetPoint(EPoints.NeededExperience));
                        if (partialLevelUps > 0)
                        {
                            Health = Player.MaxHp;
                            Mana = Player.MaxSp;
                            for (var i = 0; i < partialLevelUps; i++)
                            {
                                GiveStatusPoints();
                            }
                        }

                        CheckLevelUp();
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
                case EPoints.PlayTime:
                    Player.PlayTime += (uint) value;
                    break;
                default:
                    _logger.LogError("Failed to add point to {Point}, unsupported", point);
                    break;
            }
        }

        internal static int CalcPartialLevelUps(uint before, uint after, uint requiredForNextLevel)
        {
            if (after >= requiredForNextLevel) return 0;

            const int CHUNK_AMOUNT = 4;
            var chunk = requiredForNextLevel / CHUNK_AMOUNT;
            var beforeChunk = (int) (before / (float) chunk);
            var afterChunk = (int) (after / (float) chunk);

            return afterChunk - beforeChunk;
        }

        public override void SetPoint(EPoints point, uint value)
        {
            switch (point)
            {
                case EPoints.Level:
                    var currentLevel = GetPoint(EPoints.Level);
                    if (value > currentLevel)
                    {
                        LevelUp((int) (value - currentLevel));
                    }
                    else
                    {
                        LevelUp(-(int) (currentLevel - value));
                    }
                    break;
                case EPoints.Experience:
                    Player.Experience = value;
                    CheckLevelUp();
                    break;
                case EPoints.Gold:
                    Player.Gold = value;
                    break;
                case EPoints.PlayTime:
                    Player.PlayTime = value;
                    break;
                default:
                    _logger.LogError("Failed to set point to {Point}, unsupported", point);
                    break;
            }
        }

        private void Inventory_OnSlotChanged(object? sender, SlotChangedEventArgs args)
        {
            switch (args.Slot)
            {
                case EquipmentSlots.Weapon:
                    if (args.ItemInstance is not null)
                    {
                        var item = _itemManager.GetItem(args.ItemInstance.ItemId);
                        Player.MinWeaponDamage = item?.GetMinWeaponDamage() ?? 0;
                        Player.MaxWeaponDamage = item?.GetMaxWeaponDamage() ?? 0;
                    }
                    else
                    {
                        Player.MinWeaponDamage = 0;
                        Player.MaxWeaponDamage = 0;
                    }

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
                    return Player.MaxHp;
                case EPoints.MaxSp:
                    return Player.MaxSp;
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
                    return Player.MinWeaponDamage;
                case EPoints.MaxWeaponDamage:
                    return Player.MaxWeaponDamage;
                case EPoints.MinAttackDamage:
                    return Player.MinAttackDamage;
                case EPoints.MaxAttackDamage:
                    return Player.MaxAttackDamage;
                case EPoints.Defence:
                case EPoints.DefenceGrade:
                    return _defence;
                case EPoints.StatusPoints:
                    return Player.AvailableStatusPoints;
                case EPoints.PlayTime:
                    return (uint) TimeSpan.FromMilliseconds(Player.PlayTime).TotalMinutes;
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

            var playerManager = _scope.ServiceProvider.GetRequiredService<IPlayerManager>();
            await playerManager.SetPlayerAsync(Player);
        }

        protected override void OnNewNearbyEntity(IEntity entity)
        {
            entity.ShowEntity(Connection);
        }

        protected override void OnRemoveNearbyEntity(IEntity entity)
        {
            entity.HideEntity(Connection);
        }

        public void DropItem(ItemInstance item, byte count)
        {
            if (count > item.Count)
            {
                return;
            }

            if (item.Count == count)
            {
                RemoveItem(item);
                SendRemoveItem(item.Window, (ushort) item.Position);
                item.Set(_cacheManager, 0, 0, 0).Wait(); // TODO
            }
            else
            {
                item.Count -= count;
                item.Persist(_cacheManager).Wait(); // TODO

                SendItem(item);

                var proto = _itemManager.GetItem(item.ItemId);
                if (proto is null)
                {
                    _logger.LogCritical("Failed to find proto {ProtoId} for instanced item {ItemId}",
                        item.ItemId, item.Id);
                    return;
                }

                item = _itemManager.CreateItem(proto, count);
            }

            (Map as Map)?.AddGroundItem(item, PositionX, PositionY);
        }

        public void Pickup(IGroundItem groundItem)
        {
            if (Map is null) return;

            var item = groundItem.Item;
            if (item.ItemId == 1)
            {
                AddPoint(EPoints.Gold, (int) groundItem.Amount);
                SendPoints();
                Map.DespawnEntity(groundItem);

                return;
            }

            if (!Inventory.PlaceItem(item).Result) // TODO
            {
                SendChatInfo("No inventory space left");
                return;
            }

            SendItem(item);
            Map.DespawnEntity(groundItem);
        }

        public void DropGold(uint amount)
        {
            var proto = _itemManager.GetItem(1);

            if (proto is null)
            {
                _logger.LogCritical("Cannot find proto for gold. This must never happen");
                return;
            }

            // todo prevent crashing the server with dropping gold too often ;)

            if (amount > GetPoint(EPoints.Gold))
            {
                return; // We can't drop more gold than we have ^^
            }

            AddPoint(EPoints.Gold, -(int) amount);
            SendPoints();

            var item = _itemManager.CreateItem(proto, 1); // count will be overwritten as it's gold
            (Map as Map)?.AddGroundItem(item, PositionX, PositionY,
                amount); // todo add method to IMap interface when we have an item interface...
        }

        /// <summary>
        /// Does nothing - if you want to persist the player use <see cref="OnDespawnAsync"/>
        /// </summary>
        public override void OnDespawn()
        {
        }

        public async Task OnDespawnAsync()
        {
            await Persist();
        }

        public async Task CalculatePlayedTimeAsync()
        {
            var key = $"player:{Player.Id}:loggedInTime";
            var startSessionTime = await _cacheManager.Get<long>(key);
            var totalSessionTime = Connection.Server.ServerTime - startSessionTime;
            if (totalSessionTime <= 0) return;
            
            AddPoint(EPoints.PlayTime, (int) totalSessionTime);
        }

        public ItemInstance? GetItem(byte window, ushort position)
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

        public bool DestroyItem(ItemInstance item)
        {
            RemoveItem(item);
            if (!item.Destroy(_cacheManager).Result) // TODO
            {
                return false;
            }

            SendRemoveItem(item.Window, (ushort) item.Position);
            return true;
        }

        public void RemoveItem(ItemInstance item)
        {
            switch (item.Window)
            {
                case (byte) WindowType.Inventory:
                    if (item.Position >= Inventory.Size)
                    {
                        // Equipment
                        Inventory.EquipmentWindow.RemoveItem(item);
                        CalculateDefence();
                        SendCharacterUpdate();
                        SendPoints();
                    }
                    else
                    {
                        // Inventory
                        Inventory.RemoveItem(item);
                    }

                    break;
            }
        }

        public void SetItem(ItemInstance item, byte window, ushort position)
        {
            switch (window)
            {
                case (byte) WindowType.Inventory:
                    if (position >= Inventory.Size)
                    {
                        // Equipment
                        if (Inventory.EquipmentWindow.GetItem(position) == null)
                        {
                            Inventory.SetEquipment(item, position);
                            item.Set(_cacheManager, Player.Id, window, position).Wait(); // TODO
                            CalculateDefence();
                            SendCharacterUpdate();
                            SendPoints();
                        }
                    }
                    else
                    {
                        // Inventory
                        Inventory.PlaceItem(item, position);
                    }

                    break;
            }
        }

        public override void ShowEntity(IConnection connection)
        {
            SendCharacter(connection);
            SendCharacterAdditional(connection);
        }

        public override void HideEntity(IConnection connection)
        {
            connection.Send(new RemoveCharacter
            {
                Vid = Vid
            });
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
                Empire = Empire
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

        public void SendItem(ItemInstance item)
        {
            Debug.Assert(item.PlayerId == Player.Id);

            var p = new SetItem
            {
                Window = item.Window,
                Position = (ushort) item.Position,
                ItemId = item.ItemId,
                Count = item.Count
            };
            Connection.Send(p);
        }

        public void SendRemoveItem(byte window, ushort position)
        {
            Connection.Send(new SetItem
            {
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
                Empire = Player.Empire,
                Level = Player.Level,
                Parts = new ushort[]
                {
                    (ushort) (Inventory.EquipmentWindow.Body?.ItemId ?? 0),
                    (ushort) (Inventory.EquipmentWindow.Weapon?.ItemId ?? 0),
                    0,
                    (ushort) (Inventory.EquipmentWindow.Hair?.ItemId ?? 0)
                }
            });
        }

        public void SendCharacterUpdate()
        {
            var packet = new CharacterUpdate
            {
                Vid = Vid,
                Parts = new ushort[]
                {
                    (ushort) (Inventory.EquipmentWindow.Body?.ItemId ?? 0),
                    (ushort) (Inventory.EquipmentWindow.Weapon?.ItemId ?? 0), 0,
                    (ushort) (Inventory.EquipmentWindow.Hair?.ItemId ?? 0)
                },
                MoveSpeed = MovementSpeed,
                AttackSpeed = _attackSpeed
            };

            Connection.Send(packet);

            foreach (var entity in NearbyEntities)
            {
                if (entity is PlayerEntity p)
                {
                    p.Connection.Send(packet);
                }
            }
        }

        public void SendChatMessage(string message)
        {
            var chat = new ChatOutcoming
            {
                MessageType = ChatMessageTypes.Normal,
                Vid = Vid,
                Empire = Empire,
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
                Empire = Empire,
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
                Empire = Empire,
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

        public void Disconnect()
        {
            Inventory.OnSlotChanged -= Inventory_OnSlotChanged;
            Connection.Close();
        }

        public override string ToString()
        {
            return Player.Name + "(Player)";
        }

        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}
