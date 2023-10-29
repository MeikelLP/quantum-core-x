using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using QuantumCore.API;

namespace QuantumCore.Game.Services;

public class DropProvider : IDropProvider
{
    private readonly Dictionary<uint, DropEntry[]> _drops = new();
    private readonly Dictionary<uint, float> _itemDropMultipliers = new();
    private readonly ILogger<DropProvider> _logger;
    private readonly IItemManager _itemManager;

    public DropProvider(ILogger<DropProvider> logger, IItemManager itemManager)
    {
        _logger = logger;
        _itemManager = itemManager;
    }

    public IReadOnlyCollection<DropEntry> GetDropsForMob(uint monsterProtoId)
    {
        if (_drops.TryGetValue(monsterProtoId, out var arr))
        {
            return arr;
        }

        return Array.Empty<DropEntry>();
    }

    public float GetDropMultiplierForItem(uint itemId)
    {
        if (_itemDropMultipliers.TryGetValue(itemId, out var value))
        {
            return value;
        }

        return 1;
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await LoadDropsAsync(cancellationToken);
        await LoadItemDropModifiersAsync(cancellationToken);
    }

    private async Task LoadItemDropModifiersAsync(CancellationToken cancellationToken)
    {
        const string file = "data/etc_drop_item.txt";
        if (!File.Exists(file)) return;

        _logger.LogDebug("Loading item drop modifiers from {FilePath}", file);

        using var sr = new StreamReader(file, Encoding.GetEncoding("EUC-KR"));

        var lineIndex = 0;
        // loop while line is not null
        while((await sr.ReadLineAsync(cancellationToken))! is { } line)
        {
            var result = ParseModifierLine(line, lineIndex, file);

            if (result is not null)
            {
                _itemDropMultipliers.Add(result.Value.Key, result.Value.Value);
            }

            lineIndex++;
        }

        _logger.LogDebug("Found drop multipliers for {Count:D} items", _itemDropMultipliers.Count);
    }

    private KeyValuePair<uint, float>? ParseModifierLine(ReadOnlySpan<char> line, int lineIndex, string file)
    {
        var i = line.IndexOf('\t');

        if (i == -1)
        {
            _logger.LogDebug("Line {LineNumber} of the file {FilePath} is not valid", lineIndex + 1, file);
            return null;
        }

        var itemName = line[..i];
        var multiplier = line[(i + 1)..];

        var item = _itemManager.GetItemByName(itemName);

        if (item is null)
        {
            _logger.LogDebug("Could find item for name {ItemName}", itemName.ToString());
            return null;
        }

        if (!float.TryParse(multiplier, CultureInfo.InvariantCulture, out var multiplierValue))
        {
            _logger.LogDebug("Cannot parse multiplier value {Value} of item {ItemName}", multiplier.ToString(), itemName.ToString());
            return null;
        }

        return new KeyValuePair<uint, float>(item.Id, multiplierValue);
    }

    private async Task LoadDropsAsync(CancellationToken cancellationToken)
    {
        const string file = "data/mob_drop_item.txt";
        if (!File.Exists(file)) return;

        _logger.LogDebug("Loading drops from {FilePath}", file);

        using var sr = new StreamReader(file);
        do
        {
            var item = await ParserUtils.GetDropsAsync(sr, cancellationToken);
            if (item != null)
            {
                var newArr = item.Value.Value;
                if (_drops.TryGetValue(item.Value.Key, out var existingArr))
                {
                    var previousSize = existingArr.Length;
                    Array.Resize(ref existingArr, previousSize + newArr.Length);
                    newArr.CopyTo(existingArr, previousSize);
                }
                else
                {
                    _drops.Add(item.Value.Key, newArr);
                }
            }
        } while (!sr.EndOfStream);

        _logger.LogDebug("Found drops for {Count:D} mobs", _drops.Count);
    }
}
