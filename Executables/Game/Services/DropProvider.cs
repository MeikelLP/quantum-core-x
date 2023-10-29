using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using QuantumCore.API;

namespace QuantumCore.Game.Services;

public class DropProvider : IDropProvider
{
    private readonly Dictionary<uint, DropEntry[]> _monsterDrops = new();
    private readonly Dictionary<uint, float> _commonDrops = new();

    private readonly ILogger<DropProvider> _logger;
    private readonly IItemManager _itemManager;
    private readonly IMonsterManager _monsterManager;

    public DropProvider(ILogger<DropProvider> logger, IItemManager itemManager, IMonsterManager monsterManager)
    {
        _logger = logger;
        _itemManager = itemManager;
        _monsterManager = monsterManager;
    }

    public IReadOnlyCollection<DropEntry> GetDropsForMob(uint monsterProtoId)
    {
        if (_monsterDrops.TryGetValue(monsterProtoId, out var arr))
        {
            return arr;
        }

        return Array.Empty<DropEntry>();
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await LoadCommonDropsAsync(cancellationToken);
        await LoadDropsForMonstersAsync(cancellationToken);
    }

    /// <summary>
    /// Loads drops that are defined in mob_proto's drop_item column. The parsed file contains the default drop chance
    /// </summary>
    private async Task LoadCommonDropsAsync(CancellationToken cancellationToken)
    {
        const string file = "data/etc_drop_item.txt";
        if (!File.Exists(file)) return;

        _logger.LogDebug("Loading item drop modifiers from {FilePath}", file);

        using var sr = new StreamReader(file, Encoding.GetEncoding("EUC-KR"));

        var lineIndex = 0;
        // loop while line is not null
        while ((await sr.ReadLineAsync(cancellationToken))! is { } line)
        {
            var result = ParseCommonLine(line, lineIndex, file);

            if (result is not null)
            {
                _commonDrops.Add(result.Value.Key, result.Value.Value);
            }

            lineIndex++;
        }

        _logger.LogDebug("Found drop multipliers for {Count:D} items", _commonDrops.Count);
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
                var newArr = item.Value.Value;
                if (_monsterDrops.TryGetValue(item.Value.Key, out var existingArr))
                {
                    var previousSize = existingArr.Length;
                    Array.Resize(ref existingArr, previousSize + newArr.Length);
                    newArr.CopyTo(existingArr, previousSize);
                }
                else
                {
                    _monsterDrops.Add(item.Value.Key, newArr);
                }
            }
        } while (!sr.EndOfStream);

        foreach (var monster in _monsterManager.GetMonsters())
        {
            if (monster.DropItemId == 0) continue;

            if (_commonDrops.TryGetValue(monster.DropItemId, out var chance))
            {
                if (_monsterDrops.TryGetValue(monster.Id, out var existingArr))
                {
                    Array.Resize(ref existingArr, existingArr.Length + 1);
                    existingArr[^1] = new DropEntry(monster.DropItemId, chance);
                }
                else
                {
                    _monsterDrops.Add(monster.Id, new []
                    {
                        new DropEntry(monster.DropItemId, chance)
                    });
                }
            }

        }

        _logger.LogDebug("Found drops for {Count:D} mobs", _monsterDrops.Count);
    }
}
