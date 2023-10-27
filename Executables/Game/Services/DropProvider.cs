using Microsoft.Extensions.Logging;

namespace QuantumCore.Game.Services;

public class DropProvider : IDropProvider
{
    private readonly Dictionary<uint, DropEntry[]> _drops = new();
    private readonly ILogger<DropProvider> _logger;

    public DropProvider(ILogger<DropProvider> logger)
    {
        _logger = logger;
    }

    public IReadOnlyCollection<DropEntry> GetDropsForMob(uint monsterProtoId)
    {
        if (_drops.TryGetValue(monsterProtoId, out var arr))
        {
            return arr;
        }

        return Array.Empty<DropEntry>();
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
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
