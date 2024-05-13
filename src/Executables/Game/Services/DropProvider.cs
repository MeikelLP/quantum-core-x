using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Utils;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Services;

public class DropProvider : IDropProvider
{
    private readonly Dictionary<uint, ImmutableArray<MonsterDropEntry>> _monsterDrops = new();
    private readonly Dictionary<uint, float> _simpleMobDrops = new();

    private readonly ILogger<DropProvider> _logger;
    private readonly IConfiguration _configuration;
    private readonly IItemManager _itemManager;
    private readonly IMonsterManager _monsterManager;
    private static readonly Encoding FileEncoding = Encoding.GetEncoding("EUC-KR");
    private int[] _bossPercentageDeltas;
    private int[] _mobPercentageDeltas;

    public DropProvider(ILogger<DropProvider> logger, IConfiguration configuration, IItemManager itemManager, IMonsterManager monsterManager)
    {
        _logger = logger;
        _configuration = configuration;
        _itemManager = itemManager;
        _monsterManager = monsterManager;
    }

    public ImmutableArray<MonsterDropEntry> GetDropsForMob(uint monsterProtoId)
    {
        if (_monsterDrops.TryGetValue(monsterProtoId, out var arr))
        {
            return arr;
        }

        return new ImmutableArray<MonsterDropEntry>();
    }

    public ImmutableArray<CommonDropEntry> CommonDrops { get; private set; } = ImmutableArray<CommonDropEntry>.Empty;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await Task.WhenAll(
            LoadCommonMobDropsAsync(cancellationToken),
            LoadDeltaPercentagesAsync(cancellationToken),
            LoadDropsForMonstersAsync(cancellationToken),
            LoadSimpleMobDropsAsync(cancellationToken)
        );
    }

    private Task LoadDeltaPercentagesAsync(CancellationToken cancellationToken = default)
    {
        _bossPercentageDeltas = _configuration.GetSection("drops:delta:boss")
            .AsEnumerable(true)
            .OrderBy(x => x.Value)
            .Select(x => int.Parse(x.Value!))
            .ToArray();
        
        _mobPercentageDeltas = _configuration.GetSection("drops:delta:normal")
            .AsEnumerable(true)
            .OrderBy(x => x.Value)
            .Select(x => int.Parse(x.Value!))
            .ToArray();
        return Task.CompletedTask;
    }

    public (int deltaPercentage, int dropRange) CalculateDropPercentages(IPlayerEntity player, MonsterEntity monster)
    {
        var deltaPercentage = 0;
        var dropRange = 0;

        var levelDropDelta = (int) (monster.GetPoint(EPoints.Level) + 15 - player.GetPoint(EPoints.Level));

        deltaPercentage = monster is {IsStone: false, Rank: >= EEntityRank.Boss}
            ? _bossPercentageDeltas[MathUtils.MinMax(0, levelDropDelta, _bossPercentageDeltas.Length)]
            : _mobPercentageDeltas[MathUtils.MinMax(0, levelDropDelta, _mobPercentageDeltas.Length)];
        
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
    /// The parsed file contains the default drop chances for items.
    /// Drops defined in mob_proto's drop_item column get their chance from this file.
    /// </summary>
    private async Task LoadSimpleMobDropsAsync(CancellationToken cancellationToken)
    {
        const string file = "data/etc_drop_item.txt";
        if (!File.Exists(file)) return;

        _logger.LogDebug("Loading item drop modifiers from {FilePath}", file);

        using var sr = new StreamReader(file, FileEncoding);

        var lineIndex = 0;
        // loop while line is not null
        var simpleMobDrops = new Dictionary<uint, float>();
        while ((await sr.ReadLineAsync(cancellationToken))! is { } line)
        {
            var result = ParseCommonLine(line, lineIndex, file);

            if (result is not null)
            {
                simpleMobDrops.Add(result.Value.Key, result.Value.Value);
            }

            lineIndex++;
        }

        foreach (var mobId in _monsterDrops.Keys.ToArray()) // copy
        {
            var mob = _monsterManager.GetMonster(mobId);
            if (mob is null)
            {
                _logger.LogWarning("Cannot load simple drop for mob {Id} because that mob does not exist", mobId);
                continue;
            }
            if (mob.DropItemId != 0 &&
                simpleMobDrops.TryGetValue(mob.DropItemId, out var chance))
            {
                _monsterDrops[mobId] = _monsterDrops[mobId].Add(new MonsterDropEntry(mob.DropItemId, chance));
            }
        }

        _logger.LogDebug("Found simple drops for {Count:D} items", _simpleMobDrops.Count);
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

        // divide by 100 to make a human percentage to math percentage
        return new KeyValuePair<uint, float>(item.Id, multiplierValue / 100);
    }

    private async Task LoadDropsForMonstersAsync(CancellationToken cancellationToken)
    {
        const string file = "data/mob_drop_item.txt";
        if (!File.Exists(file)) return;

        _logger.LogDebug("Loading drops from {FilePath}", file);

        using var sr = new StreamReader(file);
        do
        {
            var item = await ParserUtils.GetDropsForBlockAsync(sr, cancellationToken);
            if (item != null)
            {
                if (_monsterDrops.TryGetValue(item.Value.Key, out var list))
                {
                    _monsterDrops[item.Value.Key] = list.AddRange(item.Value.Value);
                }
                else
                {
                    _monsterDrops.Add(item.Value.Key, item.Value.Value.ToImmutableArray());
                }
            }
        } while (!sr.EndOfStream);

        foreach (var monster in _monsterManager.GetMonsters())
        {
            if (monster.DropItemId == 0) continue;

            if (_simpleMobDrops.TryGetValue(monster.DropItemId, out var chance))
            {
                if (_monsterDrops.TryGetValue(monster.Id, out var list))
                {
                    _monsterDrops[monster.Id] = list.Add(new MonsterDropEntry(monster.DropItemId, chance));
                }
                else
                {
                    var arr = new []
                    {
                        new MonsterDropEntry(monster.DropItemId, chance)
                    }.ToImmutableArray();
                    _monsterDrops.Add(monster.Id, arr);
                }
            }

        }

        _logger.LogDebug("Found drops for {Count:D} mobs", _monsterDrops.Count);
    }
}
