using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Core.Timekeeping;
using QuantumCore.API.Extensions;
using QuantumCore.API.Game.Guild;
using QuantumCore.API.Game.Skills;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.Types.Combat;
using QuantumCore.API.Game.Types.Entities;
using QuantumCore.API.Game.Types.Items;
using QuantumCore.API.Game.Types.Monsters;
using QuantumCore.API.Game.Types.Players;
using QuantumCore.API.Game.Types.Skills;
using QuantumCore.API.Game.World;
using QuantumCore.API.Packets;
using QuantumCore.API.Packets.Guild;
using QuantumCore.Extensions;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Persistence;
using QuantumCore.Game.PlayerUtils;
using QuantumCore.Game.Skills;

namespace QuantumCore.Game.World.Entities;

public class PlayerEntity : Entity, IPlayerEntity, IDisposable
{
    public override EEntityType Type => EEntityType.PLAYER;

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
                case EPlayerClass.WARRIOR:
                    return EAntiFlags.WARRIOR;
                case EPlayerClass.NINJA:
                    return EAntiFlags.ASSASSIN;
                case EPlayerClass.SURA:
                    return EAntiFlags.SURA;
                case EPlayerClass.SHAMAN:
                    return EAntiFlags.SHAMAN;
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
                case EPlayerGender.MALE:
                    return EAntiFlags.MALE;
                case EPlayerGender.FEMALE:
                    return EAntiFlags.FEMALE;
                default:
                    return 0;
            }
        }
    }

    private uint _defence;

    private const int HEALTH_REGEN_INTERVAL = 3 * 1000;
    private const int MANA_REGEN_INTERVAL = 3 * 1000;
    private ServerTimestamp? _lastHealthRegenTime;
    private ServerTimestamp? _lastManaRegenTime;
    private readonly IItemManager _itemManager;
    private readonly IJobManager _jobManager;
    private readonly IExperienceManager _experienceManager;
    private readonly IQuestManager _questManager;
    private readonly ICacheManager _cacheManager;
    private readonly IWorld _world;
    private readonly ILogger<PlayerEntity> _logger;
    private readonly IServiceScope _scope;
    private readonly IItemRepository _itemRepository;
    private bool _isDisposed;

    public PlayerEntity(PlayerData player, IGameConnection connection, IItemManager itemManager,
        IJobManager jobManager,
        IExperienceManager experienceManager, IAnimationManager animationManager,
        IQuestManager questManager, ICacheManager cacheManager, IWorld world, ILogger<PlayerEntity> logger,
        IServiceProvider serviceProvider)
#pragma warning disable CA1062 // validate parameter before use - impossible here
        : base(animationManager, world.GenerateVid())
#pragma warning restore CA1062
    {
        ArgumentNullException.ThrowIfNull(player);
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
        Inventory = new Inventory(itemManager, _cacheManager, _itemRepository, player.Id,
            WindowType.INVENTORY, InventoryConstants.DEFAULT_INVENTORY_WIDTH,
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
        if (info is null)
        {
            return 0;
        }

        return info.StartSp + info.SpPerIq * point + info.SpPerLevel * level;
    }

    private static uint GetMaxHp(IJobManager jobManager, EPlayerClassGendered playerClass, byte level, uint point)
    {
        var info = jobManager.Get(playerClass);
        if (info is null)
        {
            return 0;
        }

        return info.StartHp + info.HpPerHt * point + info.HpPerLevel * level;
    }

    public async Task LoadAsync()
    {
        await Inventory.LoadAsync();
        await QuickSlotBar.LoadAsync();
        Player.MaxHp = GetMaxHp(_jobManager, Player.PlayerClass, Player.Level, Player.Ht);
        Player.MaxSp = GetMaxSp(_jobManager, Player.PlayerClass, Player.Level, Player.Iq);
        Health = (int)GetPoint(EPoint.MAX_HP); // todo: cache hp of player
        Mana = (int)GetPoint(EPoint.MAX_SP);
        await LoadPermGroupsAsync();
        await Skills.LoadAsync();
        var guildManager = _scope.ServiceProvider.GetRequiredService<IGuildManager>();
        Guild = await guildManager.GetGuildForPlayerAsync(Player.Id);
        Player.GuildId = Guild?.Id;
        _questManager.InitializePlayer(this);

        CalculateDefence();
        CalculateMovement();
        CalculateAttackSpeed();
    }

    public async Task ReloadPermissionsAsync()
    {
        Groups.Clear();
        await LoadPermGroupsAsync();
    }

    private async Task LoadPermGroupsAsync()
    {
        var commandPermissionRepository = _scope.ServiceProvider.GetRequiredService<ICommandPermissionRepository>();
        var playerId = Player.Id;

        var groups = await commandPermissionRepository.GetGroupsForPlayerAsync(playerId);

        foreach (var group in groups)
        {
            Groups.Add(group);
        }
    }

    public T? GetQuestInstance<T>() where T : class, IQuest
    {
        var id = typeof(T).FullName;
        if (id is null)
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
            localMap.IsAttr(new Coordinates((uint)x, (uint)y), EMapAttributes.BLOCK | EMapAttributes.OBJECT))
        {
            _logger.LogDebug(
                "Not allowed to move character {Name} to map position ({X}, {Y}) with attributes Block or Object", Name,
                x, y);
            return;
        }

        PositionX = x;
        PositionY = y;

        // Reset movement info
        Stop();
    }

    private void CalculateDefence()
    {
        _defence = GetPoint(EPoint.LEVEL) + (uint)Math.Floor(0.8 * GetPoint(EPoint.HT));

        foreach (var slot in Enum.GetValues<EquipmentSlot>())
        {
            var item = Inventory.EquipmentWindow.GetItem(slot);
            if (item is null) continue;
            var proto = _itemManager.GetItem(item.ItemId);
            if (proto is null || !proto.IsType(EItemType.ARMOR)) continue;

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
            if (item is null) continue;
            var proto = _itemManager.GetItem(item.ItemId);
            if (proto is null || !proto.IsType(EItemType.ARMOR)) continue;

            modifier += proto.GetApplyValue(EApplyType.MOV_SPEED);
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
            if (item is null) continue;
            var proto = _itemManager.GetItem(item.ItemId);
            if (proto is null) continue;

            modifier += proto.GetApplyValue(EApplyType.ATTACK_SPEED);
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

        var dead = new CharacterDead { Vid = Vid };
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
            Connection.Send(new GuildName { Id = Guild.Id, Name = Guild.Name });
        }
    }

    public async Task RefreshGuildAsync()
    {
        var guildManager = _scope.ServiceProvider.GetRequiredService<IGuildManager>();
        Guild = await guildManager.GetGuildForPlayerAsync(Player.Id);
        Player.GuildId = Guild?.Id;
        SendGuildInfo();
        this.SendCharacterUpdate();
    }

    public void Respawn(bool town)
    {
        if (!Dead)
        {
            return;
        }

        Shop?.Close(this);

        Dead = false;

        if (town && TryGetTownCoordinates(Player.Empire, Map!, out var coordinates))
        {
            Move(coordinates.Value);
        }

        // todo spawn with invisible affect

        this.SendChatCommand("CloseRestartWindow");
        Connection.SetPhase(EPhase.GAME);

        var remove = new RemoveCharacter { Vid = Vid };

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
        this.SendPoints();
    }

    private static bool TryGetTownCoordinates(EEmpire playerEmpire, IMap map,
        [NotNullWhen(true)] out Coordinates? coordinates)
    {
        coordinates = null;
        var townCoordinates = map.TownCoordinates;
        if (townCoordinates is not null)
        {
            coordinates = playerEmpire switch
            {
                EEmpire.CHUNJO => townCoordinates.Chunjo,
                EEmpire.JINNO => townCoordinates.Jinno,
                EEmpire.SHINSOO => townCoordinates.Shinsoo,
                _ => throw new ArgumentOutOfRangeException(nameof(playerEmpire),
                    $"Can't get empire coordinates for empire {playerEmpire}")
            };
            return true;
        }

        return false;
    }

    public void RecalculateStatusPoints()
    {
        var shouldHavePoints = (uint)((Player.Level - 1) * 3);
        var steps = (byte)Math.Floor(
            GetPoint(EPoint.EXPERIENCE) / (double)GetPoint(EPoint.NEEDED_EXPERIENCE) * 4);
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
        var exp = GetPoint(EPoint.EXPERIENCE);
        var needed = GetPoint(EPoint.NEEDED_EXPERIENCE);

        if (needed > 0 && exp >= needed)
        {
            SetPoint(EPoint.EXPERIENCE, exp - needed);
            LevelUp();

            if (!CheckLevelUp())
            {
                this.SendPoints();
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

        AddPoint(EPoint.SKILL, level);
        AddPoint(EPoint.SUB_SKILL, level < 10 ? 0 : level - Math.Max((int)Player.Level, 9));

        Player.Level = (byte)(Player.Level + level);

        // todo: animation (I think this actually is a quest sent by the server on character login and not an actual packet at this stage)

        foreach (var entity in NearbyEntities)
        {
            if (entity is not IPlayerEntity other) continue;
            this.SendCharacterAdditional(other.Connection);
        }

        RecalculateStatusPoints();
        this.SendPoints();
    }

    public uint CalculateAttackDamage(uint baseDamage)
    {
        var attackStatus = _jobManager.Get(Player.PlayerClass)?.AttackStatus;

        if (attackStatus is null) return 0;

        var levelBonus = GetPoint(EPoint.LEVEL) * 2;
        var statusBonus = (
            4 * GetPoint(EPoint.ST) +
            2 * GetPoint(attackStatus.Value)
        ) / 3;
        var weaponDamage = baseDamage * 2;

        return levelBonus + (statusBonus + weaponDamage) * GetHitRate() / 100;
    }

    public uint GetHitRate()
    {
        var b = (GetPoint(EPoint.DX) * 4 + GetPoint(EPoint.LEVEL) * 2) / 6;
        return 100 * ((b > 90 ? 90 : b) + 210) / 300;
    }

    public override void Update(TickContext ctx)
    {
        if (Map is null) return; // We don't have a map yet so we aren't spawned

        base.Update(ctx);

        var hpOrSpChanged = false;

        var maxHp = GetPoint(EPoint.MAX_HP);
        if (Health < maxHp && !Dead)
        {
            if (!_lastHealthRegenTime.HasValue)
            {
                // start counting interval only from first viable reset
                _lastHealthRegenTime = ctx.Timestamp;
            }
            else if (ctx.ElapsedSince(_lastHealthRegenTime.Value) > TimeSpan.FromMilliseconds(HEALTH_REGEN_INTERVAL))
            {
                var factor = State == EEntityState.IDLE ? 0.05 : 0.01;
                Health = Math.Min((int)maxHp, Health + 15 + (int)(maxHp * factor));
                hpOrSpChanged = true;

                _lastHealthRegenTime = ctx.Timestamp;
            }
        }

        var maxSp = GetPoint(EPoint.MAX_SP);
        if (Mana < maxSp && !Dead)
        {
            if (!_lastManaRegenTime.HasValue)
            {
                // start counting interval only from first viable reset
                _lastManaRegenTime = ctx.Timestamp;
            }
            else if (ctx.ElapsedSince(_lastManaRegenTime.Value) > TimeSpan.FromMilliseconds(MANA_REGEN_INTERVAL))
            {
                var factor = State == EEntityState.IDLE ? 0.05 : 0.01;
                Mana = Math.Min((int)maxSp, Mana + 15 + (int)(maxSp * factor));
                hpOrSpChanged = true;

                _lastManaRegenTime = ctx.Timestamp;
            }
        }

        if (hpOrSpChanged)
        {
            this.SendPoints();
        }
    }

    public override EBattleType GetBattleType()
    {
        return EBattleType.MELEE;
    }

    public override int GetMinDamage()
    {
        var weapon = Inventory.EquipmentWindow.Weapon;
        if (weapon is null) return 0;
        var item = _itemManager.GetItem(weapon.ItemId);
        if (item is null) return 0;
        return item.Values[3];
    }

    public override int GetMaxDamage()
    {
        var weapon = Inventory.EquipmentWindow.Weapon;
        if (weapon is null) return 0;
        var item = _itemManager.GetItem(weapon.ItemId);
        if (item is null) return 0;
        return item.Values[4];
    }

    public override int GetBonusDamage()
    {
        var weapon = Inventory.EquipmentWindow.Weapon;
        if (weapon is null) return 0;
        var item = _itemManager.GetItem(weapon.ItemId);
        if (item is null) return 0;
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
            case EPoint.LEVEL:
                LevelUp(value);
                break;
            case EPoint.EXPERIENCE:
                if (_experienceManager.GetNeededExperience((byte)GetPoint(EPoint.LEVEL)) == 0)
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
                    var partialLevelUps = CalcPartialLevelUps(before, GetPoint(EPoint.EXPERIENCE),
                        GetPoint(EPoint.NEEDED_EXPERIENCE));
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
            case EPoint.GOLD:
                var gold = Player.Gold + value;
                Player.Gold = (uint)Math.Min(uint.MaxValue, Math.Max(0, gold));
                break;
            case EPoint.ST:
                Player.St += (byte)value;
                break;
            case EPoint.DX:
                Player.Dx += (byte)value;
                break;
            case EPoint.HT:
                Player.Ht += (byte)value;
                break;
            case EPoint.IQ:
                Player.Iq += (byte)value;
                break;
            case EPoint.HP:
                if (value <= 0)
                {
                    // 0 gets ignored by client
                    // Setting the Hp to 0 does not register as killing the player
                }
                else if (value > GetPoint(EPoint.MAX_HP))
                {
                    Health = GetPoint(EPoint.MAX_HP);
                }
                else
                {
                    Health = value;
                }

                break;
            case EPoint.SP:
                if (value <= 0)
                {
                    // 0 gets ignored by client
                }
                else if (value > GetPoint(EPoint.MAX_SP))
                {
                    Mana = GetPoint(EPoint.MAX_SP);
                }
                else
                {
                    Mana = value;
                }

                break;
            case EPoint.STATUS_POINTS:
                Player.AvailableStatusPoints += (uint)value;
                break;
            case EPoint.SKILL:
                Player.AvailableSkillPoints += (uint)value;
                break;
            case EPoint.PLAY_TIME:
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
            case EPoint.LEVEL:
                var currentLevel = GetPoint(EPoint.LEVEL);
                LevelUp((int)(value - currentLevel));
                break;
            case EPoint.EXPERIENCE:
                Player.Experience = value;
                CheckLevelUp();
                break;
            case EPoint.GOLD:
                Player.Gold = value;
                break;
            case EPoint.PLAY_TIME:
                Player.PlayTime = value;
                break;
            case EPoint.SKILL:
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
            case EquipmentSlot.WEAPON:
                if (args.ItemInstance is not null)
                {
                    var item = _itemManager.GetItem(args.ItemInstance.ItemId);
                    Player.MinWeaponDamage = item?.MinWeaponDamage ?? 0;
                    Player.MaxWeaponDamage = item?.MaxWeaponDamage ?? 0;
                }
                else
                {
                    Player.MinWeaponDamage = 0;
                    Player.MaxWeaponDamage = 0;
                }

                break;
            case EquipmentSlot.BODY:
                if (args.ItemInstance is not null)
                {
                    Player.BodyPart = args.ItemInstance.ItemId;
                }
                else
                {
                    Player.BodyPart = 0;
                }

                break;
            case EquipmentSlot.HAIR:
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
            case EPoint.LEVEL:
                return Player.Level;
            case EPoint.EXPERIENCE:
                return Player.Experience;
            case EPoint.NEEDED_EXPERIENCE:
                return _experienceManager.GetNeededExperience(Player.Level);
            case EPoint.HP:
                return (uint)Health;
            case EPoint.SP:
                return (uint)Mana;
            case EPoint.MAX_HP:
                return Player.MaxHp;
            case EPoint.MAX_SP:
                return Player.MaxSp;
            case EPoint.ST:
                return Player.St;
            case EPoint.HT:
                return Player.Ht;
            case EPoint.DX:
                return Player.Dx;
            case EPoint.IQ:
                return Player.Iq;
            case EPoint.ATTACK_SPEED:
                return AttackSpeed;
            case EPoint.MOVE_SPEED:
                return MovementSpeed;
            case EPoint.GOLD:
                return Player.Gold;
            case EPoint.MIN_WEAPON_DAMAGE:
                return Player.MinWeaponDamage;
            case EPoint.MAX_WEAPON_DAMAGE:
                return Player.MaxWeaponDamage;
            case EPoint.MIN_ATTACK_DAMAGE:
                return Player.MinAttackDamage;
            case EPoint.MAX_ATTACK_DAMAGE:
                return Player.MaxAttackDamage;
            case EPoint.DEFENCE:
            case EPoint.DEFENCE_GRADE:
                return _defence;
            case EPoint.STATUS_POINTS:
                return Player.AvailableStatusPoints;
            case EPoint.PLAY_TIME:
                return (uint)TimeSpan.FromMilliseconds(Player.PlayTime).TotalMinutes;
            case EPoint.SKILL:
                return Player.AvailableSkillPoints;
            case EPoint.SUB_SKILL:
                return 1;
            default:
                return 0;
        }
    }

    private async Task PersistAsync()
    {
        await QuickSlotBar.PersistAsync();

        Player.PositionX = PositionX;
        Player.PositionY = PositionY;

        await Skills.PersistAsync();

        var playerManager = _scope.ServiceProvider.GetRequiredService<IPlayerManager>();
        await playerManager.SetPlayerAsync(Player);
    }

    protected override void OnNewNearbyEntity(IEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        entity.ShowEntity(Connection);
    }

    protected override void OnRemoveNearbyEntity(IEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        entity.HideEntity(Connection);
    }

    public async Task DropItemAsync(ItemInstance item, byte count)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (count > item.Count)
        {
            return;
        }

        if (item.Count == count)
        {
            RemoveItem(item);
            this.SendRemoveItem(item.Window, (ushort)item.Position);
            await _itemRepository.DeletePlayerItemAsync(_cacheManager, item.PlayerId, item.ItemId);
        }
        else
        {
            item.Count -= count;
            await item.PersistAsync(_itemRepository);

            this.SendItem(item);

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

    public async Task PickupAsync(IGroundItem groundItem)
    {
        ArgumentNullException.ThrowIfNull(groundItem);
        if (Map is null) return;

        var item = groundItem.Item;
        if (item.ItemId == 1)
        {
            AddPoint(EPoint.GOLD, (int)groundItem.Amount);
            this.SendPoints();
            Map.DespawnEntity(groundItem);

            return;
        }

        if (groundItem.OwnerName is not null && !string.Equals(groundItem.OwnerName, Name))
        {
            this.SendChatInfo("This item is not yours");
            return;
        }

        if (!await Inventory.PlaceItemAsync(item)) // TODO
        {
            this.SendChatInfo("No inventory space left");
            return;
        }

        var itemName = _itemManager.GetItem(item.ItemId)?.TranslatedName ?? "Unknown";
        this.SendChatInfo($"You picked up {groundItem.Amount}x {itemName}");

        this.SendItem(item);
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

        if (amount > GetPoint(EPoint.GOLD))
        {
            return; // We can't drop more gold than we have ^^
        }

        AddPoint(EPoint.GOLD, -(int)amount);
        this.SendPoints();

        var item = _itemManager.CreateItem(proto); // count will be overwritten as it's gold
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
        await PersistAsync();
    }

    public int GetMobItemRate()
    {
        // todo: implement server rates, and premium server rates
        if (GetPremiumRemainSeconds(EPremiumType.ITEM) > 0)
            return 100;
        return 100_000_000;
    }

    public int GetPremiumRemainSeconds(EPremiumType type)
    {
        _logger.LogTrace("GetPremiumRemainSeconds not implemented yet");
        return 0; // todo: implement premium system
    }

    public bool IsUsableSkillMotion(ESkill motion)
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
            var item = Inventory.EquipmentWindow.GetItem(EquipmentSlot.UNIQUE1);
            if (item is not null && item.ItemId == itemProtoId)
            {
                return true;
            }
        }
        {
            var item = Inventory.EquipmentWindow.GetItem(EquipmentSlot.UNIQUE2);
            if (item is not null && item.ItemId == itemProtoId)
            {
                return true;
            }
        }

        return false;
    }

    public async Task CalculatePlayedTimeAsync()
    {
        var key = $"player:{Player.Id}:loggedInTime";
        var startSessionElapsed = TimeSpan.FromMilliseconds(
            await _cacheManager.Server.GetAsync<long>(key)
        );
        var currentElapsed = Connection.Server.Clock.Elapsed;
        var totalSessionTime = currentElapsed - startSessionElapsed;
        if (totalSessionTime <= TimeSpan.Zero) return;

        AddPoint(EPoint.PLAY_TIME, (int)totalSessionTime.TotalMilliseconds);
    }

    public ItemInstance? GetItem(WindowType window, ushort position)
    {
        switch (window)
        {
            case WindowType.INVENTORY:
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

    public bool IsSpaceAvailable(ItemInstance item, WindowType window, ushort position)
    {
        switch (window)
        {
            case WindowType.INVENTORY:
                if (position >= Inventory.Size)
                {
                    // Equipment
                    // Make sure item fits in equipment window
                    if (IsEquippable(item) && Inventory.EquipmentWindow.IsSuitable(_itemManager, item, position))
                    {
                        return Inventory.EquipmentWindow.GetItem(position) is null;
                    }
                }

                // Inventory
                return Inventory.IsSpaceAvailable(item, position);
        }

        return false;
    }

    public bool IsEquippable(ItemInstance item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var proto = _itemManager.GetItem(item.ItemId);
        if (proto is null)
        {
            // Proto for item not found
            return false;
        }

        if (proto.WearFlags == 0 && !proto.IsType(EItemType.COSTUME))
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
            if (limit.Type == (byte)ELimitType.LEVEL)
            {
                if (Player.Level < limit.Value)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public async Task<bool> DestroyItemAsync(ItemInstance item)
    {
        ArgumentNullException.ThrowIfNull(item);
        RemoveItem(item);
        if (!await item.DestroyAsync(_cacheManager))
        {
            return false;
        }

        this.SendRemoveItem(item.Window, (ushort)item.Position);
        return true;
    }

    public void RemoveItem(ItemInstance item)
    {
        ArgumentNullException.ThrowIfNull(item);
        switch (item.Window)
        {
            case WindowType.INVENTORY:
                if (item.Position >= Inventory.Size)
                {
                    // Equipment
                    Inventory.RemoveEquipment(item);
                    CalculateDefence();
                    CalculateMovement();
                    CalculateAttackSpeed();
                    this.SendCharacterUpdate();
                    this.SendPoints();
                }
                else
                {
                    // Inventory
                    Inventory.RemoveItem(item);
                }

                break;
        }
    }

    public async Task SetItemAsync(ItemInstance item, WindowType window, ushort position)
    {
        ArgumentNullException.ThrowIfNull(item);
        switch (window)
        {
            case WindowType.INVENTORY:
                if (position >= Inventory.Size)
                {
                    // Equipment
                    if (Inventory.EquipmentWindow.GetItem(position) is null)
                    {
                        Inventory.SetEquipment(item, position);
                        await item.SetAsync(_cacheManager, Player.Id, window, position, _itemRepository);
                        CalculateDefence();
                        CalculateMovement();
                        CalculateAttackSpeed();
                        this.SendCharacterUpdate();
                        this.SendPoints();
                    }
                }
                else
                {
                    // Inventory
                    await Inventory.PlaceItemAsync(item, position);
                }

                break;
        }
    }

    public override void ShowEntity(IConnection connection)
    {
        SendGuildInfo();
        this.SendCharacter(connection);
        this.SendCharacterAdditional(connection);
    }

    public override void HideEntity(IConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);
        connection.Send(new RemoveCharacter { Vid = Vid });
        SendOfflineNotice(connection);
    }

    private void SendOfflineNotice(IConnection connection)
    {
        var guildId = Player.GuildId;
        if (guildId is not null && connection is IGameConnection gameConnection &&
            gameConnection.Player!.Player.GuildId == guildId)
        {
            connection.Send(new GuildMemberOfflinePacket { PlayerId = Player.Id });
        }
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
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            _scope.Dispose();
        }

        _isDisposed = true;
    }
}