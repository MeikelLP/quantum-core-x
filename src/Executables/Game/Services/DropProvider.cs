﻿using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Drops;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.PlayerUtils;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Services;

public class DropProvider : IDropProvider
{
    private readonly Dictionary<uint, MonsterItemGroup> _monsterDrops = new();
    private readonly Dictionary<uint, DropItemGroup> _itemDrops = new();

    #if DEBUG
    private const bool DropDebug = true;
    #else
    private const bool DropDebug = false;
    #endif
    
    private readonly ILogger<DropProvider> _logger;
    private readonly IItemManager _itemManager;
    private static readonly Encoding FileEncoding = Encoding.GetEncoding("EUC-KR");
    private readonly DropOptions _options;

    public DropProvider(ILogger<DropProvider> logger, IOptions<GameOptions> gameOptions, IItemManager itemManager)
    {
        _logger = logger;
        _options = gameOptions.Value.Drops;
        _itemManager = itemManager;
    }
    
    public MonsterItemGroup? GetMonsterDropsForMob(uint monsterProtoId)
    {
        return _monsterDrops.TryGetValue(monsterProtoId, out var arr) ? arr : null;
    }

    public DropItemGroup? GetDropItemsGroupForMob(uint monsterProtoId)
    {
        return _itemDrops.TryGetValue(monsterProtoId, out var arr) ? arr : null;
    }

    public ImmutableArray<CommonDropEntry> CommonDrops { get; private set; } = ImmutableArray<CommonDropEntry>.Empty;

    public ImmutableArray<EtcItemDropEntry> EtcDrops { get; private set; } = ImmutableArray<EtcItemDropEntry>.Empty;
    public ImmutableArray<LevelItemGroup> LevelDrops { get; private set; } = ImmutableArray<LevelItemGroup>.Empty;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Task.AwaitAll
        await LoadCommonMobDropsAsync(cancellationToken);
        await LoadSimpleMobDropsAsync(cancellationToken);
        await LoadDropsForMonstersAsync();
    }

    #region Loading

    private async Task LoadCommonMobDropsAsync(CancellationToken cancellationToken)
    {
        const string file = "data/common_drop_item.txt";
        if (!File.Exists(file)) return;

        _logger.LogDebug("Loading common drops from {FilePath}", file);

        using var sr = new StreamReader(file, FileEncoding);

        CommonDrops = await ParserUtils.GetCommonDropsAsync(sr, cancellationToken);

        _logger.LogDebug("Found {Count:D} common drops", CommonDrops.Length);
    }

    /// <summary>
    /// The parsed file contains an additional multiplier drop chance for some items.
    /// </summary>
    private async Task LoadSimpleMobDropsAsync(CancellationToken cancellationToken)
    {
        const string file = "data/etc_drop_item.txt";
        if (!File.Exists(file)) return;

        _logger.LogDebug("Loading item drop modifiers from {FilePath}", file);

        using var sr = new StreamReader(file, FileEncoding);

        var lineIndex = 0;
        // loop while line is not null
        var etcDrops = new List<EtcItemDropEntry>();
        while ((await sr.ReadLineAsync(cancellationToken))! is { } line)
        {
            var result = ParseCommonLine(line, lineIndex, file);

            if (result is not null)
            {
                etcDrops.Add(new EtcItemDropEntry(result.Value.Key, result.Value.Value));
            }

            lineIndex++;
        }
        
        EtcDrops = etcDrops.ToImmutableArray();

        _logger.LogDebug("Found simple drops for {Count:D} items", EtcDrops.Length);
    }

    private KeyValuePair<uint, float>? ParseCommonLine(ReadOnlySpan<char> line, int lineIndex, string file)
    {
        var i = line.IndexOf('\t');

        if (i == -1)
        {
            _logger.LogDebug("Line {LineNumber} of the file {FilePath} is not valid", lineIndex + 1, file);
            return null;
        }

        var itemName = line[..i];
        var chance = line[(i + 1)..];

        var item = _itemManager.GetItemByName(itemName);

        if (item is null)
        {
            _logger.LogDebug("Could find item for name {ItemName}", itemName.ToString());
            return null;
        }

        if (!float.TryParse(chance, CultureInfo.InvariantCulture, out var multiplierValue))
        {
            _logger.LogDebug("Cannot parse multiplier value {Value} of item {ItemName}", chance.ToString(),
                itemName.ToString());
            return null;
        }

        return new KeyValuePair<uint, float>(item.Id, multiplierValue * 10000.0f); // 1 to 1000
    }
    
    private async Task LoadDropsForMonstersAsync()
    {
        const string file = "data/mob_drop_item.txt";
        if (!File.Exists(file)) return;
        
        _logger.LogDebug("Loading drops from {FilePath}", file);

        using var sr = new StreamReader(file, FileEncoding);
        
        var parsedGroups = new List<MonsterDropContainer>();
        var mobGroups = await ParserUtils.ParseFileGroups(sr);
        
        foreach (var mobGroup in mobGroups)
        {
            var container = ParserUtils.ParseMobGroup(mobGroup, _itemManager);
            if (container != null)
            {
                parsedGroups.Add(container);
            }
            else
            {
                // We're skipping groups with no data. But if there is data, we throw an exception because it shouldn't occur.
                if (mobGroup.Data.Count > 0) throw new InvalidOperationException("Invalid group format");
            }
        }

        var monsters = parsedGroups.OfType<MonsterItemGroup>();
        foreach (var monster in monsters)
        {
            if (_monsterDrops.ContainsKey(monster.MonsterProtoId))
            {
                _logger.LogWarning("Duplicate monster drop entry for {MonsterProtoId}", monster.MonsterProtoId);
                continue;
            }
            _monsterDrops[monster.MonsterProtoId] = monster;
        }
        
        var dropItems = parsedGroups.OfType<DropItemGroup>();
        foreach (var dropItem in dropItems)
        {
            if (_itemDrops.ContainsKey(dropItem.MonsterProtoId))
            {
                _itemDrops[dropItem.MonsterProtoId].Drops.AddRange(dropItem.Drops);
                continue;
            }
            _itemDrops[dropItem.MonsterProtoId] = dropItem;
        }
        
        LevelDrops = [..parsedGroups.OfType<LevelItemGroup>()];

        _logger.LogDebug("Found {Count:D} group drops", parsedGroups.Count());
    }

    #endregion

    public (int deltaPercentage, int dropRange) CalculateDropPercentages(IPlayerEntity player, MonsterEntity monster)
    {
        var deltaPercentage = 0;
        var dropRange = 0;

        var levelDropDelta = (int) (monster.GetPoint(EPoints.Level) + 15 - player.GetPoint(EPoints.Level));

        deltaPercentage = monster is {IsStone: false, Rank: >= EMonsterLevel.Boss}
            ? (int) _options.Delta.Boss[MathUtils.MinMax(0, levelDropDelta, _options.Delta.Boss.Count - 1)]
            : (int) _options.Delta.Normal[MathUtils.MinMax(0, levelDropDelta, _options.Delta.Normal.Count - 1)];
        
        if (1 == CoreRandom.GenerateInt32(1, 50001))
            deltaPercentage += 1000;
        else if (1 == CoreRandom.GenerateInt32(1, 10001))
            deltaPercentage += 500;
        
        _logger.LogDebug("CalculateDropPercentages for level: {Level} rank: {Rank} percentage: {DeltaPercentage}", 
            player.GetPoint(EPoints.Level), monster.Rank.ToString(), deltaPercentage);
        
        deltaPercentage = deltaPercentage * player.GetMobItemRate() / 100;
        
        if (player.GetPoint(EPoints.MallItemBonus) > 0)
        {
            deltaPercentage += (int) (deltaPercentage * player.GetPoint(EPoints.MallItemBonus) / 100);
        }
        
        const int UNIQUE_GROUP_DOUBLE_ITEM = 10002; // todo: magic numbers
        const int UNIQUE_ITEM_DOUBLE_ITEM = 70043;  // todo: magic numbers
        
        // Premium
        if (player.GetPremiumRemainSeconds(EPremiumTypes.Item) > 0 || player.HasUniqueGroupItemEquipped(UNIQUE_GROUP_DOUBLE_ITEM))
        {
            deltaPercentage += deltaPercentage;
        }
        // Premium end

        var bonus = 0;
        if (player.HasUniqueItemEquipped(UNIQUE_ITEM_DOUBLE_ITEM) && player.GetPremiumRemainSeconds(EPremiumTypes.Item) > 0)
        {
            // irremovable gloves + mall item bonus
            bonus = 100;
            _logger.LogDebug("Player has irremovable gloves and mall item bonus");
        }
        else if (player.HasUniqueItemEquipped(UNIQUE_ITEM_DOUBLE_ITEM) 
                 || (player.HasUniqueGroupItemEquipped(UNIQUE_GROUP_DOUBLE_ITEM) && player.GetPremiumRemainSeconds(EPremiumTypes.Item) > 0))
        {
            // irremovable gloves OR removeable gloves + mall item bonus
            bonus = 50;
            _logger.LogDebug("Player has irremovable gloves OR removeable gloves and mall item bonus");
        }
        
        var itemDropBonus = (int) Math.Min(100, player.GetPoint(EPoints.ItemDropBonus));

        var empireBonusDrop = 0; // todo: implement server / empire rates
        
        dropRange = 4_000_000;
        dropRange = dropRange * 100 / (100 + empireBonusDrop + bonus + itemDropBonus);
        
        return (deltaPercentage, dropRange);
    }

    #region Drop Calculations

    public List<ItemInstance> CalculateCommonDropItems(IPlayerEntity player, MonsterEntity monster, int delta, int range)
    {
        var items = new List<ItemInstance>();
        
        var commonDrops = this.GetPossibleCommonDropsForPlayer(player);
        foreach (var drop in commonDrops)
        {
            var percent = (drop.Chance * delta) / 100;
            var target = CoreRandom.GenerateInt32(1, range + 1);
                
            if (DropDebug)
            {
                var realPercent = percent / range * 100;
                _logger.LogTrace("Drop chance for {Name} ({MobProtoId}) is {RealPercent}%", 
                    monster.Proto.TranslatedName, monster.Proto.Id, realPercent);
            }
                
            if (percent >= target)
            {
                var itemProto = _itemManager.GetItem(drop.ItemProtoId);
                if (itemProto is null)
                {
                    _logger.LogWarning("Could not find item proto for {ItemProtoId}", drop.ItemProtoId);
                    continue;
                }
                
                _logger.LogDebug("Monster {Name} ({MobProtoId}) dropped an item group ({Item})", 
                    monster.Proto.TranslatedName, monster.Proto.Id, itemProto.TranslatedName);
                    
                var itemInstance = _itemManager.CreateItem(itemProto);

                if ((EItemType) itemProto.Type ==  EItemType.Polymorph)
                {
                    if (monster.Proto.PolymorphItemId == itemProto.Id)
                    {
                        // todo: set item socket 0 value to monster proto id (when ItemInstance have sockets implemented)
                    }
                }
                    
                items.Add(itemInstance);
            }
        }
        return items;
    }

    public List<ItemInstance> CalculateDropItemGroupItems(MonsterEntity monster, int delta, int range)
    {
        var items = new List<ItemInstance>();
        
        var mobItemGroupDrops = GetDropItemsGroupForMob(monster.Proto.Id);
        if (mobItemGroupDrops is not null)
        {
            foreach (var drop in mobItemGroupDrops.Drops)
            {
                var percent = drop.Chance * delta / 100;
                var target = CoreRandom.GenerateInt32(1, range + 1);
                    
                if (DropDebug)
                {
                    var realPercent = percent / range * 100;
                    _logger.LogTrace("Drop chance for {Name} ({MobProtoId}) is {RealPercent}%", 
                        monster.Proto.TranslatedName, monster.Proto.Id, realPercent);
                }
                    
                if (percent >= target)
                {
                    var itemProto = _itemManager.GetItem(drop.ItemProtoId);
                    if (itemProto is null)
                    {
                        _logger.LogWarning("Could not find item proto for {ItemProtoId}", drop.ItemProtoId);
                        continue;
                    }
                    
                    _logger.LogDebug("Monster {Name} ({MobProtoId}) dropped an item group ({Item})", 
                        monster.Proto.TranslatedName, monster.Proto.Id, itemProto.TranslatedName);
                        
                    if ((EItemType) itemProto.Type ==  EItemType.Polymorph)
                    {
                        if (monster.Proto.PolymorphItemId == itemProto.Id)
                        {
                            // todo: set item socket 0 value to monster proto id (when ItemInstance have sockets implemented)
                        }
                    }
                        
                    var itemInstance = _itemManager.CreateItem(itemProto, (byte) drop.Amount);
                    items.Add(itemInstance);
                }
            }
        }
        return items;
    }

    public List<ItemInstance> CalculateMobDropItemGroupItems(IPlayerEntity player, MonsterEntity monster, int delta, int range)
    {
        var items = new List<ItemInstance>();
        
        var mobDrops = this.GetPossibleMobDropsForPlayer(monster.Proto.Id);
        if (mobDrops is not {IsEmpty: false}) return items;
        
        var percent = 40000 * delta / mobDrops.MinKillCount;
        var target = CoreRandom.GenerateInt32(1, range + 1);

        if (DropDebug)
        {
            var realPercent = (float) percent / range * 100;
            _logger.LogTrace("Drop chance for {Name} ({MobProtoId}) is {RealPercent}%",
                monster.Proto.TranslatedName, monster.Proto.Id, realPercent);
        }
                
        if (percent >= target)
        {
            var randomDrop = mobDrops.GetDrop();
            var itemProto = _itemManager.GetItem(randomDrop?.ItemProtoId ?? 0);
            if (itemProto is not null)
            {
                _logger.LogDebug("Monster {Name} ({MobProtoId}) dropped an item group ({Item})", 
                    monster.Proto.TranslatedName, monster.Proto.Id, itemProto.TranslatedName);
                var itemInstance = _itemManager.CreateItem(itemProto, (byte) randomDrop!.Amount);
                items.Add(itemInstance);
            }
            else
            {
                _logger.LogWarning("Could not find item proto for {ItemProtoId}", randomDrop?.ItemProtoId);
            }
        }
        return items;
    }

    public List<ItemInstance> CalculateLevelDropItems(IPlayerEntity player, MonsterEntity monster, int delta, int range)
    {
        var items = new List<ItemInstance>();
        
        var levelDrops = this.GetPossibleLevelDropsForPlayer(player);
        
        foreach (var levelDrop in levelDrops)
        {
            foreach (var drop in levelDrop.Drops)
            {
                var percent = drop.Chance;
                var target = CoreRandom.GenerateInt32(1, 1_000_000 + 1);
                    
                if (DropDebug)
                {
                    var realPercent = percent / range * 100;
                    _logger.LogTrace("Drop chance for {Name} ({MobProtoId}) is {RealPercent}%",
                        monster.Proto.TranslatedName, monster.Proto.Id, realPercent);
                }
                
                if (percent >= target)
                {
                    var itemProto = _itemManager.GetItem(drop.ItemProtoId);
                    if (itemProto is null)
                    {
                        _logger.LogWarning("Could not find item proto for {ItemProtoId}", drop.ItemProtoId);
                        continue;
                    }
                        
                    _logger.LogDebug("Monster {Name} ({MobProtoId}) dropped an item group ({Item})", 
                        monster.Proto.TranslatedName, monster.Proto.Id, itemProto.TranslatedName);
                            
                    var itemInstance = _itemManager.CreateItem(itemProto, (byte) drop.Amount);
                    items.Add(itemInstance);
                }
            }
        }
        return items;
    }

    public List<ItemInstance> CalculateEtcDropItems(MonsterEntity monster, int delta, int range)
    {
        var items = new List<ItemInstance>();
        var etcDrops = EtcDrops.Where(x => x.ItemProtoId == monster.Proto.DropItemId);
        foreach (var drop in etcDrops)
        {
            var percent = drop.Multiplier * delta / 100;
            var target = CoreRandom.GenerateInt32(1, range + 1);
                
            if (DropDebug)
            {
                var realPercent = percent / range * 100;
                _logger.LogTrace("Drop chance for {Name} ({MobProtoId}) is {RealPercent}%", 
                    monster.Proto.TranslatedName, monster.Proto.Id, realPercent);
            }
                
            if (percent >= target)
            {
                var itemProto = _itemManager.GetItem(drop.ItemProtoId);
                if (itemProto is null)
                {
                    _logger.LogWarning("Could not find item proto for {ItemProtoId}", drop.ItemProtoId);
                    continue;
                }
                
                _logger.LogDebug("Monster {Name} ({MobProtoId}) dropped an item group ({Item})", 
                    monster.Proto.TranslatedName, monster.Proto.Id, itemProto.TranslatedName);
                    
                var itemInstance = _itemManager.CreateItem(itemProto);
                items.Add(itemInstance);
            }
        }
        return items;
    }

    private (int SpiritStoneId, int Chance) DetermineDropMetinStone(MonsterEntity monster)
    {
        var metinStoneId = (int) monster.Proto.Id;
            
        // calculate the lower bound
        var index = 0;
        var upper = _options.MetinStones.Count - 1;
        while (index < upper)
        {
            var middle = index + (upper - index) / 2;
            if (_options.MetinStones[middle].MonsterProtoId.CompareTo(metinStoneId) < 0)
            {
                index = middle + 1;
            }
            else
            {
                upper = middle;
            }
        }
            
        if (index >= _options.MetinStones.Count || _options.MetinStones[index].MonsterProtoId != metinStoneId)
        {
            return (0, 0);
        }

        var stoneInfo = _options.MetinStones[index];
        
        // ItemProtoId of the spirit stone
        var spiritStoneId = (int) _options.SpiritStones[CoreRandom.GenerateInt32(0, _options.SpiritStones.Count)];
        var rank = CoreRandom.GenerateInt32(0, 101);
            
        // determine the rank (+0, +1, +2...)
        foreach (var rankChance in stoneInfo.RankChance)
        {
            if (rank <= rankChance)
            {
                break;
            }

            rank -= rankChance;
            spiritStoneId += 100; // each rank is 100+ higher than the previous one
        }
        
        return (spiritStoneId, stoneInfo.DropChance);
    }
    
    public List<ItemInstance> CalculateMetinDropItems(MonsterEntity monster, int delta, int range)
    {
        if (!monster.IsStone) return [];

        var (spiritStoneId, chance) = DetermineDropMetinStone(monster);
        
        if (spiritStoneId == 0) return [];

        var percent = chance * delta * 400;
        var target = CoreRandom.GenerateInt32(1, range + 1);
        
        if (DropDebug)
        {
            var realPercent = percent / range * 100;
            _logger.LogTrace("Drop chance for {Name} ({MobProtoId}) is {RealPercent}%", 
                monster.Proto.TranslatedName, monster.Proto.Id, realPercent);
        }

        if (percent >= target)
        {
            var itemProto = _itemManager.GetItem((uint) spiritStoneId);
            if (itemProto is null)
            {
                _logger.LogWarning("Could not find item proto for {ItemProtoId}", spiritStoneId);
                return [];
            }
            
            _logger.LogDebug("Monster {Name} ({MobProtoId}) dropped an item group ({Item})", 
                monster.Proto.TranslatedName, monster.Proto.Id, itemProto.TranslatedName);
                    
            var itemInstance = _itemManager.CreateItem(itemProto);
            return [itemInstance];
        }
        
        return [];
    }

    #endregion

    
}
