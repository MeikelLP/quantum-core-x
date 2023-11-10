using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using QuantumCore.API;

namespace QuantumCore.Game.Services;

public class DropProvider : IDropProvider
{
    private readonly Dictionary<uint, ImmutableArray<MonsterDropEntry>> _monsterDrops = new();
    private readonly Dictionary<uint, float> _simpleMobDrops = new();

    private readonly ILogger<DropProvider> _logger;
    private readonly IItemManager _itemManager;
    private readonly IMonsterManager _monsterManager;

    public DropProvider(ILogger<DropProvider> logger, IItemManager itemManager, IMonsterManager monsterManager)
    {
        _logger = logger;
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

    public ImmutableArray<CommonDropEntry> CommonDrops { get; private set; }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await LoadDropsForMonstersAsync(cancellationToken);
        await LoadSimpleMobDropsAsync(cancellationToken);
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

        using var sr = new StreamReader(file, Encoding.GetEncoding("EUC-KR"));

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

        _logger.LogDebug("Found drop multipliers for {Count:D} items", _simpleMobDrops.Count);
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
