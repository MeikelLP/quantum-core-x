using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Extensions;
using QuantumCore.API.Game.Guild;
using QuantumCore.API.Game.Skills;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Caching;
using QuantumCore.Extensions;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets;
using QuantumCore.Game.Packets.Guild;
using QuantumCore.Game.Persistence;
using QuantumCore.Game.PlayerUtils;
using QuantumCore.Game.Skills;
using QuantumCore.Game.World;
using QuantumCore.Game.Services;

namespace QuantumCore.Game.World.Entities
{
    public class PlayerEntity : Entity, IPlayerEntity, IDisposable
    {
        public override EEntityType Type => EEntityType.Player;

        public string Name => Player.Name;
        public IGameConnection Connection { get; }
        public PlayerData Player { get; private set; }
        public GuildData? Guild { get; private set; }
        public IInventory Inventory { get; private set; }
        public IList<Guid> Groups { get; private set; }
        public IShop? Shop { get; set; }
        public IQuickSlotBar QuickSlotBar { get; }
        public IPlayerSkills Skills { get; private set; }
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
                switch (Player.PlayerClass.GetClass())
                {
                    case EPlayerClass.Warrior:
                        return EAntiFlags.Warrior;
                    case EPlayerClass.Ninja:
                        return EAntiFlags.Assassin;
                    case EPlayerClass.Sura:
                        return EAntiFlags.Sura;
                    case EPlayerClass.Shaman:
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
                switch (Player.PlayerClass.GetGender())
                {
                    case EPlayerGender.Male:
                        return EAntiFlags.Male;
                    case EPlayerGender.Female:
                        return EAntiFlags.Female;
                    default:
                        return 0;
                }
            }
        }

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
        private readonly IItemRepository _itemRepository;

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
            _itemRepository = _scope.ServiceProvider.GetRequiredService<IItemRepository>();
            Inventory = new Inventory(itemManager, _cacheManager, _logger, _itemRepository, player.Id,
                (byte)WindowType.Inventory, InventoryConstants.DEFAULT_INVENTORY_WIDTH,
                InventoryConstants.DEFAULT_INVENTORY_HEIGHT, InventoryConstants.DEFAULT_INVENTORY_PAGES);
            Inventory.OnSlotChanged += Inventory_OnSlotChanged;
            Player = player;
            Empire = player.Empire;
            PositionX = player.PositionX;
            PositionY = player.PositionY;
            QuickSlotBar = ActivatorUtilities.CreateInstance<QuickSlotBar>(_scope.ServiceProvider, this);
            Skills = ActivatorUtilities.CreateInstance<PlayerSkills>(_scope.ServiceProvider, this);

            MovementSpeed = PlayerConstants.DEFAULT_MOVEMENT_SPEED;
            AttackSpeed = PlayerConstants.DEFAULT_ATTACK_SPEED;
            EntityClass = (uint)player.PlayerClass;

            Groups = new List<Guid>();
        }

        private static uint GetMaxSp(IJobManager jobManager, EPlayerClassGendered playerClass, byte level, uint point)
        {
            var info = jobManager.Get(playerClass);
            if (info == null)
            {
                return 0;
            }

            return info.StartSp + info.SpPerIq * point + info.SpPerLevel * level;
        }

        private static uint GetMaxHp(IJobManager jobManager, EPlayerClassGendered playerClass, byte level, uint point)
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
            Health = (int)GetPoint(EPoint.MaxHp); // todo: cache hp of player
            Mana = (int)GetPoint(EPoint.MaxSp);
            await LoadPermGroups();
            await Skills.LoadAsync();
            var guildManager = _scope.ServiceProvider.GetRequiredService<IGuildManager>();
            Guild = await guildManager.GetGuildForPlayerAsync(Player.Id);
            Player.GuildId = Guild?.Id;
            _questManager.InitializePlayer(this);

            CalculateDefence();
            CalculateMovement();
            CalculateAttackSpeed();
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

            return (T)Quests[id];
        }

        private void Warp(Coordinates position) => Warp((int)position.X, (int)position.Y);

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
                ServerAddress = BitConverter.ToInt32(host.Ip.GetAddressBytes()),
                ServerPort = host.Port
            };
            Connection.Send(packet);
        }

        public void Move(Coordinates position) => Move((int)position.X, (int)position.Y);

        public override void Move(int x, int y)
        {
            if (Map is null) return;
            if (PositionX == x && PositionY == y) return;

            if (!Map.IsPositionInside(x, y))
            {
                Warp(x, y);
                return;
            }

            if (Map is Map localMap &&
                localMap.IsAttr(new Coordinates((uint)x, (uint)y), EMapAttribute.Block | EMapAttribute.Object))
            {
                _logger.LogDebug("Not allowed to move character {Name} to map position ({X}, {Y}) with attributes Block or Object", Name, x, y);
                return;
            }

            PositionX = x;
            PositionY = y;

            // Reset movement info
            Stop();
        }

        private void CalculateDefence()
        {
            _defence = GetPoint(EPoint.Level) + (uint)Math.Floor(0.8 * GetPoint(EPoint.Ht));

            foreach (var slot in Enum.GetValues<EquipmentSlot>())
            {
                var item = Inventory.EquipmentWindow.GetItem(slot);
                if (item == null) continue;
                var proto = _itemManager.GetItem(item.ItemId);
                if (proto is null || !proto.IsType(EItemType.Armor)) continue;

                _defence += (uint)proto.Values[1] + (uint)proto.Values[5] * 2;
            }

            _logger.LogDebug("Calculate defence value for {Name}, result: {Defence}", Name, _defence);

            // todo add defence bonus from quests
        }

        private void CalculateMovement()
        {
            MovementSpeed = PlayerConstants.DEFAULT_MOVEMENT_SPEED;
            float modifier = 0;
            foreach (var slot in Enum.GetValues<EquipmentSlot>())
            {
                var item = Inventory.EquipmentWindow.GetItem(slot);
                if (item == null) continue;
                var proto = _itemManager.GetItem(item.ItemId);
                if (proto is null || !proto.IsType(EItemType.Armor)) continue;

                modifier += proto.GetApplyValue(EApplyType.MovSpeed);
            }

            var calculatedSpeed = MovementSpeed * (1 + modifier / 100);

            MovementSpeed = (byte)Math.Min(calculatedSpeed, byte.MaxValue);
            _logger.LogDebug("Calculate Movement value for {Name}, result: {MovementSpeed}", Name, MovementSpeed);
        }

        private void CalculateAttackSpeed()
        {
            AttackSpeed = PlayerConstants.DEFAULT_ATTACK_SPEED;
            float modifier = 0;
            foreach (var slot in Enum.GetValues<EquipmentSlot>())
            {
                var item = Inventory.EquipmentWindow.GetItem(slot);
                if (item == null) continue;
                var proto = _itemManager.GetItem(item.ItemId);
                if (proto == null) continue;

                modifier += proto.GetApplyValue(EApplyType.AttackSpeed);
            }

            AttackSpeed = (byte)Math.Min(AttackSpeed * (1 + modifier / 100), byte.MaxValue);
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

        private void SendGuildInfo()
        {
            if (Guild is not null)
            {
                var onlineMemberIds = _world.GetGuildMembers(Guild.Id).Select(x => x.Player.Id).ToArray();
                Connection.SendGuildMembers(Guild.Members, onlineMemberIds);
                Connection.SendGuildRanks(Guild.Ranks);
                Connection.SendGuildInfo(Guild);
                Connection.Send(new GuildName {Id = Guild.Id, Name = Guild.Name});
            }
        }

        public async Task RefreshGuildAsync()
        {
            var guildManager = _scope.ServiceProvider.GetRequiredService<IGuildManager>();
            Guild = await guildManager.GetGuildForPlayerAsync(Player.Id);
            Player.GuildId = Guild?.Id;
            SendGuildInfo();
            SendCharacterUpdate();
        }

        public void Respawn(bool town)
        {
            if (!Dead)
            {
                return;
            }

            Shop?.Close(this);

            Dead = false;

            if (town)
            {
                var townCoordinates = Map!.TownCoordinates;
                if (townCoordinates is not null)
                {
                    Move(Player.Empire switch
                    {
                        EEmpire.Chunjo => townCoordinates.Chunjo,
                        EEmpire.Jinno => townCoordinates.Jinno,
                        EEmpire.Shinsoo => townCoordinates.Shinsoo,
                        _ => throw new ArgumentOutOfRangeException(nameof(Player.Empire),
                            $"Can't get empire coordinates for empire {Player.Empire}")
                    });
                }
            }

            // todo spawn with invisible affect

            SendChatCommand("CloseRestartWindow");
            Connection.SetPhase(EPhase.Game);

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

            Health = PlayerConstants.RESPAWN_HEALTH;
            Mana = PlayerConstants.RESPAWN_MANA;
            SendPoints();
        }

        public void RecalculateStatusPoints()
        {
            var shouldHavePoints = (uint)((Player.Level - 1) * 3);
            var steps = (byte)Math.Floor(
                GetPoint(EPoint.Experience) / (double)GetPoint(EPoint.NeededExperience) * 4);
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
            var exp = GetPoint(EPoint.Experience);
            var needed = GetPoint(EPoint.NeededExperience);

            if (needed > 0 && exp >= needed)
            {
                SetPoint(EPoint.Experience, exp - needed);
                LevelUp();

                if (!CheckLevelUp())
                {
                    SendPoints();
                }

                return true;
            }

            RecalculateStatusPoints();
            return false;
        }

        private void LevelUp(int level = 1)
        {
            if (Player.Level + level > _experienceManager.MaxLevel)
            {
                return;
            }

            AddPoint(EPoint.Skill, level);
            AddPoint(EPoint.SubSkill, level < 10 ? 0 : level - Math.Max((int)Player.Level, 9));

            Player.Level = (byte)(Player.Level + level);

            // todo: animation (I think this actually is a quest sent by the server on character login and not an actual packet at this stage)

            foreach (var entity in NearbyEntities)
            {
                if (entity is not IPlayerEntity other) continue;
                SendCharacterAdditional(other.Connection);
            }

            RecalculateStatusPoints();
            SendPoints();
        }

        public uint CalculateAttackDamage(uint baseDamage)
        {
            var attackStatus = _jobManager.Get(Player.PlayerClass)?.AttackStatus;

            if (attackStatus is null) return 0;

            var levelBonus = GetPoint(EPoint.Level) * 2;
            var statusBonus = (
                4 * GetPoint(EPoint.St) +
                2 * GetPoint(attackStatus.Value)
            ) / 3;
            var weaponDamage = baseDamage * 2;

            return levelBonus + (statusBonus + weaponDamage) * GetHitRate() / 100;
        }

        public uint GetHitRate()
        {
            var b = (GetPoint(EPoint.Dx) * 4 + GetPoint(EPoint.Level) * 2) / 6;
            return 100 * ((b > 90 ? 90 : b) + 210) / 300;
        }

        public override void Update(double elapsedTime)
        {
            if (Map == null) return; // We don't have a map yet so we aren't spawned

            base.Update(elapsedTime);

            var maxHp = GetPoint(EPoint.MaxHp);
            if (Health < maxHp && !Dead)
            {
                _healthRegenTime -= elapsedTime;
                if (_healthRegenTime <= 0)
                {
                    var factor = State == EEntityState.Idle ? 0.05 : 0.01;
                    Health = Math.Min((int)maxHp, Health + 15 + (int)(maxHp * factor));
                    SendPoints();

                    _healthRegenTime += HealthRegenInterval;
                }
            }

            var maxSp = GetPoint(EPoint.MaxSp);
            if (Mana < maxSp && !Dead)
            {
                _manaRegenTime -= elapsedTime;
                if (_manaRegenTime <= 0)
                {
                    var factor = State == EEntityState.Idle ? 0.05 : 0.01;
                    Mana = Math.Min((int)maxSp, Mana + 15 + (int)(maxSp * factor));
                    SendPoints();

                    _manaRegenTime += ManaRegenInterval;
                }
            }

            _persistTime += (int)elapsedTime;
            if (_persistTime > PersistInterval)
            {
                Persist().Wait(); // TODO
                _persistTime -= PersistInterval;
            }
        }

        public override EBattleType GetBattleType()
        {
            return EBattleType.Melee;
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

        public override void AddPoint(EPoint point, int value)
        {
            if (value == 0)
            {
                return;
            }

            switch (point)
            {
                case EPoint.Level:
                    LevelUp(value);
                    break;
                case EPoint.Experience:
                    if (_experienceManager.GetNeededExperience((byte)GetPoint(EPoint.Level)) == 0)
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
                        Player.Experience = (uint)(Player.Experience + value);
                    }

                    if (value > 0)
                    {
                        var partialLevelUps = CalcPartialLevelUps(before, GetPoint(EPoint.Experience),
                            GetPoint(EPoint.NeededExperience));
                        if (partialLevelUps > 0)
                        {
                            Health = Player.MaxHp;
                            Mana = Player.MaxSp;
                            for (var i = 0; i < partialLevelUps; i++)
                            {
                                RecalculateStatusPoints();
                            }
                        }

                        CheckLevelUp();
                    }

                    break;
                case EPoint.Gold:
                    var gold = Player.Gold + value;
                    Player.Gold = (uint)Math.Min(uint.MaxValue, Math.Max(0, gold));
                    break;
                case EPoint.St:
                    Player.St += (byte)value;
                    break;
                case EPoint.Dx:
                    Player.Dx += (byte)value;
                    break;
                case EPoint.Ht:
                    Player.Ht += (byte)value;
                    break;
                case EPoint.Iq:
                    Player.Iq += (byte)value;
                    break;
                case EPoint.Hp:
                    if (value <= 0)
                    {
                        // 0 gets ignored by client
                        // Setting the Hp to 0 does not register as killing the player
                    }
                    else if (value > GetPoint(EPoint.MaxHp))
                    {
                        Health = GetPoint(EPoint.MaxHp);
                    }
                    else
                    {
                        Health = value;
                    }

                    break;
                case EPoint.Sp:
                    if (value <= 0)
                    {
                        // 0 gets ignored by client
                    }
                    else if (value > GetPoint(EPoint.MaxSp))
                    {
                        Mana = GetPoint(EPoint.MaxSp);
                    }
                    else
                    {
                        Mana = value;
                    }

                    break;
                case EPoint.StatusPoints:
                    Player.AvailableStatusPoints += (uint)value;
                    break;
                case EPoint.Skill:
                    Player.AvailableSkillPoints += (uint)value;
                    break;
                case EPoint.PlayTime:
                    Player.PlayTime += (uint)value;
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
            var beforeChunk = (int)(before / (float)chunk);
            var afterChunk = (int)(after / (float)chunk);

            return afterChunk - beforeChunk;
        }

        public override void SetPoint(EPoint point, uint value)
        {
            switch (point)
            {
                case EPoint.Level:
                    var currentLevel = GetPoint(EPoint.Level);
                    LevelUp((int)(value - currentLevel));
                    break;
                case EPoint.Experience:
                    Player.Experience = value;
                    CheckLevelUp();
                    break;
                case EPoint.Gold:
                    Player.Gold = value;
                    break;
                case EPoint.PlayTime:
                    Player.PlayTime = value;
                    break;
                case EPoint.Skill:
                    Player.AvailableSkillPoints = (byte)value;
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
                case EquipmentSlot.Weapon:
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
                case EquipmentSlot.Body:
                    if (args.ItemInstance is not null)
                    {
                        Player.BodyPart = args.ItemInstance.ItemId;
                    }
                    else
                    {
                        Player.BodyPart = 0;
                    }
                
                    break;
                case EquipmentSlot.Hair:
                    if (args.ItemInstance is not null)
                    {
                        Player.HairPart = args.ItemInstance.GetHairPartOffsetForClient(Player.PlayerClass.GetClass());
                    }
                    else
                    {
                        Player.HairPart = 0;
                    }
                
                    break;
                    
            }
        }

        public override uint GetPoint(EPoint point)
        {
            switch (point)
            {
                case EPoint.Level:
                    return Player.Level;
                case EPoint.Experience:
                    return Player.Experience;
                case EPoint.NeededExperience:
                    return _experienceManager.GetNeededExperience(Player.Level);
                case EPoint.Hp:
                    return (uint)Health;
                case EPoint.Sp:
                    return (uint)Mana;
                case EPoint.MaxHp:
                    return Player.MaxHp;
                case EPoint.MaxSp:
                    return Player.MaxSp;
                case EPoint.St:
                    return Player.St;
                case EPoint.Ht:
                    return Player.Ht;
                case EPoint.Dx:
                    return Player.Dx;
                case EPoint.Iq:
                    return Player.Iq;
                case EPoint.AttackSpeed:
                    return AttackSpeed;
                case EPoint.MoveSpeed:
                    return MovementSpeed;
                case EPoint.Gold:
                    return Player.Gold;
                case EPoint.MinWeaponDamage:
                    return Player.MinWeaponDamage;
                case EPoint.MaxWeaponDamage:
                    return Player.MaxWeaponDamage;
                case EPoint.MinAttackDamage:
                    return Player.MinAttackDamage;
                case EPoint.MaxAttackDamage:
                    return Player.MaxAttackDamage;
                case EPoint.Defence:
                case EPoint.DefenceGrade:
                    return _defence;
                case EPoint.StatusPoints:
                    return Player.AvailableStatusPoints;
                case EPoint.PlayTime:
                    return (uint)TimeSpan.FromMilliseconds(Player.PlayTime).TotalMinutes;
                case EPoint.Skill:
                    return Player.AvailableSkillPoints;
                case EPoint.SubSkill:
                    return 1;
                default:
                    if (Enum.GetValues<EPoint>().Contains(point))
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

            await Skills.PersistAsync();

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
                SendRemoveItem(item.Window, (ushort)item.Position);
                _itemRepository.DeletePlayerItemAsync(_cacheManager, item.PlayerId, item.ItemId).Wait(); // TODO
            }
            else
            {
                item.Count -= count;
                item.Persist(_itemRepository).Wait(); // TODO

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
                AddPoint(EPoint.Gold, (int)groundItem.Amount);
                SendPoints();
                Map.DespawnEntity(groundItem);

                return;
            }

            if (groundItem.OwnerName != null && !string.Equals(groundItem.OwnerName, Name))
            {
                SendChatInfo("This item is not yours");
                return;
            }

            if (!Inventory.PlaceItem(item).Result) // TODO
            {
                SendChatInfo("No inventory space left");
                return;
            }

            var itemName = _itemManager.GetItem(item.ItemId)?.TranslatedName ?? "Unknown";
            SendChatInfo($"You picked up {groundItem.Amount}x {itemName}");

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

            if (amount > GetPoint(EPoint.Gold))
            {
                return; // We can't drop more gold than we have ^^
            }

            AddPoint(EPoint.Gold, -(int)amount);
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

        public int GetMobItemRate()
        {
            // todo: implement server rates, and premium server rates
            if (GetPremiumRemainSeconds(EPremiumType.Item) > 0)
                return 100;
            return 100_000_000;
        }

        public int GetPremiumRemainSeconds(EPremiumType type)
        {
            _logger.LogTrace("GetPremiumRemainSeconds not implemented yet");
            return 0; // todo: implement premium system
        }

        public bool IsUsableSkillMotion(int motion)
        {
            // todo: check if riding, mining or fishing
            return true;
        }

        public bool HasUniqueGroupItemEquipped(uint itemProtoId)
        {
            _logger.LogTrace("HasUniqueGroupItemEquipped not implemented yet");
            return false; // todo: implement unique group item system
        }

        public bool HasUniqueItemEquipped(uint itemProtoId)
        {
            {
                var item = Inventory.EquipmentWindow.GetItem(EquipmentSlot.Unique1);
                if (item != null && item.ItemId == itemProtoId)
                {
                    return true;
                }
            }
            {
                var item = Inventory.EquipmentWindow.GetItem(EquipmentSlot.Unique2);
                if (item != null && item.ItemId == itemProtoId)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task CalculatePlayedTimeAsync()
        {
            var key = $"player:{Player.Id}:loggedInTime";
            var startSessionTime = await _cacheManager.Server.Get<long>(key);
            var totalSessionTime = Connection.Server.ServerTime - startSessionTime;
            if (totalSessionTime <= 0) return;

            AddPoint(EPoint.PlayTime, (int)totalSessionTime);
        }

        public ItemInstance? GetItem(byte window, ushort position)
        {
            switch (window)
            {
                case (byte)WindowType.Inventory:
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
                case (byte)WindowType.Inventory:
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

            if (proto.WearFlags == 0 && !proto.IsType(EItemType.Costume))
            {
                // No wear flags -> not wearable
                return false;
            }

            // Check anti flags
            var antiFlags = (EAntiFlags)proto.AntiFlags;
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
                if (limit.Type == (byte)ELimitType.Level)
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

            SendRemoveItem(item.Window, (ushort)item.Position);
            return true;
        }

        public void RemoveItem(ItemInstance item)
        {
            switch (item.Window)
            {
                case (byte)WindowType.Inventory:
                    if (item.Position >= Inventory.Size)
                    {
                        // Equipment
                        Inventory.RemoveEquipment(item);
                        CalculateDefence();
                        CalculateMovement();
                        CalculateAttackSpeed();
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
                case (byte)WindowType.Inventory:
                    if (position >= Inventory.Size)
                    {
                        // Equipment
                        if (Inventory.EquipmentWindow.GetItem(position) == null)
                        {
                            Inventory.SetEquipment(item, position);
                            item.Set(_cacheManager, Player.Id, window, position, _itemRepository).Wait(); // TODO
                            CalculateDefence();
                            CalculateMovement();
                            CalculateAttackSpeed();
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
            SendGuildInfo();
            SendCharacter(connection);
            SendCharacterAdditional(connection);
        }

        public override void HideEntity(IConnection connection)
        {
            connection.Send(new RemoveCharacter {Vid = Vid});
            SendOfflineNotice(connection);
        }

        private void SendOfflineNotice(IConnection connection)
        {
            var guildId = Player.GuildId;
            if (guildId is not null && connection is IGameConnection gameConnection &&
                gameConnection.Player!.Player.GuildId == guildId)
            {
                connection.Send(new GuildMemberOfflinePacket {PlayerId = Player.Id});
            }
        }

        public void SendBasicData()
        {
            var details = new CharacterDetails
            {
                Vid = Vid,
                Name = Player.Name,
                Class = (ushort)Player.PlayerClass,
                PositionX = PositionX,
                PositionY = PositionY,
                Empire = Empire,
                SkillGroup = Player.SkillGroup
            };
            Connection.Send(details);
        }

        public void SendPoints()
        {
            var points = new CharacterPoints();
            for (var i = 0; i < points.Points.Length; i++)
            {
                points.Points[i] = GetPoint((EPoint)i);
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
                Window = item.Window, Position = (ushort)item.Position, ItemId = item.ItemId, Count = item.Count
            };
            Connection.Send(p);
        }

        public void SendRemoveItem(byte window, ushort position)
        {
            Connection.Send(new SetItem {Window = window, Position = position, ItemId = 0, Count = 0});
        }

        public void SendCharacter(IConnection connection)
        {
            connection.Send(new SpawnCharacter
            {
                Vid = Vid,
                CharacterType = (byte)EEntityType.Player,
                Angle = 0,
                PositionX = PositionX,
                PositionY = PositionY,
                Class = (ushort)Player.PlayerClass,
                MoveSpeed = MovementSpeed,
                AttackSpeed = AttackSpeed
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
                GuildId = Guild?.Id ?? 0,
                Parts = new ushort[]
                {
                    (ushort)(Inventory.EquipmentWindow.Body?.ItemId ?? 0),
                    (ushort)(Inventory.EquipmentWindow.Weapon?.ItemId ?? 0), 0,
                    (ushort)Inventory.EquipmentWindow.Hair.GetHairPartOffsetForClient(Player.PlayerClass.GetClass())
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
                    (ushort)(Inventory.EquipmentWindow.Body?.ItemId ?? 0),
                    (ushort)(Inventory.EquipmentWindow.Weapon?.ItemId ?? 0), 0,
                    (ushort)Inventory.EquipmentWindow.Hair.GetHairPartOffsetForClient(Player.PlayerClass.GetClass())
                },
                MoveSpeed = MovementSpeed,
                AttackSpeed = AttackSpeed,
                GuildId = Guild?.Id ?? 0
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
                MessageType = ChatMessageType.Normal, Vid = Vid, Empire = Empire, Message = message
            };
            Connection.Send(chat);
        }

        public void SendChatCommand(string message)
        {
            var chat = new ChatOutcoming
            {
                MessageType = ChatMessageType.Command, Vid = 0, Empire = Empire, Message = message
            };
            Connection.Send(chat);
        }

        public void SendChatInfo(string message)
        {
            var chat = new ChatOutcoming
            {
                MessageType = ChatMessageType.Info, Vid = 0, Empire = Empire, Message = message
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
