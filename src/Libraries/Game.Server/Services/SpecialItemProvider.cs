using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.Core.Utils;

namespace QuantumCore.Game.Services;

/// <summary>
/// Reads <c>special_item_group.txt</c>, which lists what container items can yield:
/// <code>
/// Group SomeChest
/// {
///     Vnum    10000
///     1       72001   1   1
/// }
/// </code>
/// Each content line is index, item proto id, count and weight.
/// </summary>
internal sealed class SpecialItemProvider : ISpecialItemProvider, ILoadable
{
    private const string FILE = "special_item_group.txt";

    private readonly IFileProvider _fileProvider;
    private readonly ILogger<SpecialItemProvider> _logger;

    // the game data files are written in the original korean encoding
    private readonly Encoding _fileEncoding;
    private FrozenDictionary<uint, ImmutableArray<SpecialItemEntry>> _groups =
        FrozenDictionary<uint, ImmutableArray<SpecialItemEntry>>.Empty;

    public SpecialItemProvider(IFileProvider fileProvider, ILogger<SpecialItemProvider> logger)
    {
        _fileProvider = fileProvider;
        _logger = logger;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // register korean locale
        _fileEncoding = Encoding.GetEncoding("EUC-KR");
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var file = _fileProvider.GetFileInfo(FILE);
        if (!file.Exists)
        {
            _logger.LogWarning("{Path} does not exist, container item contents not loaded", FILE);
            return;
        }

        await using var fs = file.CreateReadStream();
        using var sr = new StreamReader(fs, _fileEncoding);

        var groups = new Dictionary<uint, ImmutableArray<SpecialItemEntry>>();
        uint? vnum = null;
        var entries = new List<SpecialItemEntry>();

        while (await sr.ReadLineAsync(cancellationToken) is { } line)
        {
            var parts = line.Split('\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 0) continue;

            if (parts[0] == "}")
            {
                if (vnum is not null && entries.Count > 0)
                {
                    groups[vnum.Value] = [.. entries];
                }

                vnum = null;
                entries.Clear();
                continue;
            }

            if (parts[0].Equals("Vnum", StringComparison.OrdinalIgnoreCase))
            {
                if (parts.Length > 1 && uint.TryParse(parts[1], out var parsed)) vnum = parsed;
                continue;
            }

            // content line: index, item proto id, count, weight - anything else (Group, Type,
            // the opening brace) carries no reward
            if (parts.Length < 4) continue;
            if (!uint.TryParse(parts[1], out var itemId)) continue;
            if (!byte.TryParse(parts[2], out var count)) continue;
            if (!uint.TryParse(parts[3], out var weight)) continue;
            if (itemId == 0 || weight == 0) continue;

            entries.Add(new SpecialItemEntry(itemId, count == 0 ? (byte)1 : count, weight));
        }

        _groups = groups.ToFrozenDictionary();
        _logger.LogDebug("Found contents for {Count:D} container items", groups.Count);
    }

    public ImmutableArray<SpecialItemEntry> GetPossibleContents(uint containerItemProtoId)
    {
        return _groups.TryGetValue(containerItemProtoId, out var entries) ? entries : [];
    }

    public SpecialItemEntry? Roll(uint containerItemProtoId)
    {
        var entries = GetPossibleContents(containerItemProtoId);
        if (entries.IsEmpty) return null;

        var total = entries.Sum(x => (long)x.Weight);
        if (total <= 0) return null;

        var roll = CoreRandom.GenerateInt32(0, (int)Math.Min(total, int.MaxValue));
        var cumulative = 0L;
        foreach (var entry in entries)
        {
            cumulative += entry.Weight;
            if (roll < cumulative) return entry;
        }

        return entries[^1];
    }
}
