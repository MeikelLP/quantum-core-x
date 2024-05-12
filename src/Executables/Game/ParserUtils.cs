using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;
using EnumsNET;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Services;
using QuantumCore.Game.World;

namespace QuantumCore.Game;

internal static partial class ParserUtils
{
    private static readonly NumberFormatInfo InvNum = NumberFormatInfo.InvariantInfo;
    private const StringComparison INV_CUL = StringComparison.InvariantCultureIgnoreCase;

    public static SpawnPoint? GetSpawnFromLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return null;
        var splitted = SplitByWhitespaceRegex().Split(line.Trim());
        if (string.IsNullOrWhiteSpace(splitted[0]) || splitted[0].StartsWith("//")) return null;
        return new SpawnPoint
        {
            Type = Enums.Parse<ESpawnPointType>(splitted[0].AsSpan()[..1], true, EnumFormat.EnumMemberValue),
            IsAggressive = splitted[0].Length > 1 &&
                           splitted[0].AsSpan()[1..2].Equals("a", StringComparison.InvariantCultureIgnoreCase),
            X = int.Parse(splitted[1]),
            Y = int.Parse(splitted[2]),
            RangeX = int.Parse(splitted[3]),
            RangeY = int.Parse(splitted[4]),
            Direction = int.Parse(splitted[5]),
            // splitted[6],
            RespawnTime = ParseSecondsFromTimespanString(splitted[7].Trim()),
            Chance = short.Parse(splitted[8]),
            MaxAmount = short.Parse(splitted[9]),
            Monster = uint.Parse(splitted[10])
        };
    }

    private static int ParseSecondsFromTimespanString(ReadOnlySpan<char> str)
    {
        var value = int.Parse(str[..^1]);
        if (str.EndsWith("s", StringComparison.InvariantCultureIgnoreCase))
        {
            return value;
        }

        if (str.EndsWith("m", StringComparison.InvariantCultureIgnoreCase))
        {
            return value * 60;
        }

        if (str.EndsWith("h", StringComparison.InvariantCultureIgnoreCase))
        {
            return value * 3600;
        }

        throw new ArgumentOutOfRangeException(nameof(str), $"Don't know how to parse \"{str}\" to TimeSpan");
    }

    public static async Task<SpawnGroup?> GetSpawnGroupFromBlock(TextReader sr)
    {
        var item = new SpawnGroup();
        string? line;
        do
        {
            line = await sr.ReadLineAsync();
            if (line is null) return null; // EOS
        } while (!line.StartsWith("Group"));

        item.Name = GroupReplaceRegex().Replace(line, "", 1).Trim();
        await sr.ReadLineAsync();
        line = (await sr.ReadLineAsync())!;
        item.Id = uint.Parse(line.Replace("Vnum", "").Trim());
        line = (await sr.ReadLineAsync())!;
        item.Leader = uint.Parse(SplitLine(LeaderReplaceRegex().Replace(line, "", 1).Trim())[1].Trim());
        while ((line = (await sr.ReadLineAsync())!.Trim()) != "}")
        {
            if (string.IsNullOrWhiteSpace(line)) break;
            item.Members.Add(new SpawnMember
            {
                Id = uint.Parse(SplitByWhitespaceOrTabRegex().Split(line)[^1].Trim())
            });
        }

        return item;
    }

    /// <returns>null if end of stream</returns>
    public static async Task<KeyValuePair<uint, List<MonsterDropEntry>>?> GetDropsForBlockAsync(TextReader sr,
        CancellationToken cancellationToken = default)
    {
        string? line;
        do
        {
            line = await sr.ReadLineAsync(cancellationToken);

            if (line is null) return null; // EOS
        } while (!line.StartsWith("Group"));

        var drops = new List<MonsterDropEntry>();
        uint mob = 0;
        uint minLevel = 0;
        uint minKillCount = 0;
        while ((line = (await sr.ReadLineAsync(cancellationToken))!.Trim()) != "}")
        {
            if (string.IsNullOrWhiteSpace(line)) break;

            if (line.StartsWith("Level_limit", INV_CUL))
            {
                minLevel = uint.Parse(line.Replace("Level_limit", "", INV_CUL).Trim(), InvNum);
            }
            else if (line.StartsWith("Mob", INV_CUL))
            {
                mob = uint.Parse(line.Replace("Mob", "", INV_CUL).Trim(), InvNum);
            }
            else if (line.StartsWith("kill_drop", INV_CUL))
            {
                // may contain 0.0 or similar floating value
                minKillCount = (uint)decimal.Parse(line.Replace("kill_drop", "", INV_CUL).Trim(), InvNum);
            }

            if (StartsWithNumberRegex().IsMatch(line))
            {
                var splitted = line.Split('\t');
                if (uint.TryParse(splitted[1], InvNum, out var itemId))
                {
                    var amount = byte.Parse(splitted[2], InvNum);
                    var chance = float.Parse(splitted[3], InvNum) / 100; // to make it 0-1 not 0-100
                    drops.Add(new MonsterDropEntry(itemId, chance, minLevel, minKillCount, amount));
                }
                else
                {
                    // invalid item Id -- cannot handle -- will be ignored
                }
            }
        }

        if (mob == 0) return null; // invalid

        return new KeyValuePair<uint, List<MonsterDropEntry>>(mob, drops);
    }

    public static async Task<SpawnGroupCollection?> GetSpawnGroupCollectionFromBlock(TextReader sr)
    {
        var item = new SpawnGroupCollection();
        string? line;
        do
        {
            line = await sr.ReadLineAsync();
            if (line is null) return null; // EOS
        } while (!line.StartsWith("Group"));

        item.Name = GroupReplaceRegex().Replace(line, "", 1).Trim();
        await sr.ReadLineAsync();
        line = (await sr.ReadLineAsync())!;
        item.Id = uint.Parse(line.Replace("Vnum", "").Trim());
        while ((line = (await sr.ReadLineAsync())!.Trim()) != "}")
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var splitted = SplitByWhitespaceRegex().Split(line);
            var id = uint.Parse(splitted[1].Trim());
            var amount = splitted.Length == 2
                ? (byte) 1
                : byte.Parse(splitted[2].Trim());
            item.Groups.Add(new SpawnGroupCollectionMember
            {
                Id = id,
                Amount = amount
            });
        }

        return item;
    }

    public static async Task<ImmutableArray<CommonDropEntry>> GetCommonDropsAsync(TextReader sr, CancellationToken cancellationToken = default)
    {
        var list = new List<CommonDropEntry>();
        while (await sr.ReadLineAsync(cancellationToken) is { } line)
        {
            ParseCommonDropAndAdd(line, list);
        }

        return list.ToImmutableArray();
    }

    private static void ParseCommonDropAndAdd(ReadOnlySpan<char> line, ICollection<CommonDropEntry> list)
    {
        var trimmedLine = line.Trim();
        if (trimmedLine.StartsWith("PAWN")) return; // skip if first line - headers
        var totalRead = 0;
        CommonDropEntry? commonDrop;
        do
        {
            if (trimmedLine.IsEmpty || totalRead >= trimmedLine.Length) return;
            var startIndex = Math.Max(totalRead - 1, 0);
            commonDrop = ParseCommonDropFromLine(trimmedLine[startIndex..], out var read);
            totalRead += read;
            if (commonDrop is not null)
            {
                list.Add(commonDrop.Value);
            }
        } while (commonDrop is not null);
    }

    private static CommonDropEntry? ParseCommonDropFromLine(ReadOnlySpan<char> line, out int read)
    {
        var startIndex = 0;
        while (line.Length > startIndex && line[startIndex] == '\t')
        {
            startIndex++;
        }
        int minLevelStartIndex;
        if (!line.IsEmpty && char.IsDigit(line[startIndex]))
        {
            minLevelStartIndex = startIndex;
        }
        else
        {
            // skip label if any
            minLevelStartIndex = line[startIndex..].IndexOf('\t') + startIndex + 1;
        }

        var minLevelEndIndex = line[minLevelStartIndex..].IndexOf('\t') + minLevelStartIndex;

        var maxLevelStartIndex = line[minLevelEndIndex..].IndexOf('\t') + minLevelEndIndex + 1;
        var maxLevelEndIndex = line[maxLevelStartIndex..].IndexOf('\t') + maxLevelStartIndex;

        var percentageStartIndex = line[maxLevelEndIndex..].IndexOf('\t') + maxLevelEndIndex + 1;
        var percentageEndIndex = line[percentageStartIndex..].IndexOf('\t') + percentageStartIndex;

        var itemIdStartIndex = line[percentageEndIndex..].IndexOf('\t') + percentageEndIndex + 1;
        var itemIdEndIndex = line[itemIdStartIndex..].IndexOf('\t') + itemIdStartIndex;

        // special handling for last item
        var outOfStartIndex = line[itemIdEndIndex..].IndexOf('\t') + itemIdEndIndex + 1;
        var relativeOutOfEndIndex = line[outOfStartIndex..].IndexOf('\t');
        var outOfEndIndex = relativeOutOfEndIndex == -1
            ? line.Length
            : relativeOutOfEndIndex + outOfStartIndex;

        // if any end gave -1 it will be less than their relative start
        if (minLevelEndIndex < minLevelStartIndex ||
            maxLevelEndIndex < maxLevelStartIndex ||
            percentageEndIndex < percentageStartIndex ||
            itemIdEndIndex < itemIdStartIndex ||
            outOfEndIndex < outOfStartIndex)
        {
            // chunk invalid
            read = 0;
            return null;
        }

        var minLevel = byte.Parse(line[minLevelStartIndex..minLevelEndIndex]);
        var maxLevel = byte.Parse(line[maxLevelStartIndex..maxLevelEndIndex]);
        var percentage = float.Parse(line[percentageStartIndex..percentageEndIndex], CultureInfo.InvariantCulture); // TODO check if values are human percentage or math percentage => 50% vs 0.5
        var itemId = uint.Parse(line[itemIdStartIndex..itemIdEndIndex]);
        var outOf = uint.Parse(line[outOfStartIndex..outOfEndIndex]);

        read = outOfEndIndex + 1;

        // TODO monster level
        return new CommonDropEntry(minLevel, maxLevel, itemId, percentage / outOf);
    }

    public static string[] SplitLine(string line)
    {
        if (line.Contains('\t'))
        {
            return line.Split('\t', StringSplitOptions.RemoveEmptyEntries);
        }

        return line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    [GeneratedRegex("(?: {2,}|\\t+)")]
    private static partial Regex SplitByWhitespaceRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex SplitByWhitespaceOrTabRegex();

    [GeneratedRegex("Leader")]
    private static partial Regex LeaderReplaceRegex();

    [GeneratedRegex("Group")]
    private static partial Regex GroupReplaceRegex();

    [GeneratedRegex(@"^\d")]
    private static partial Regex StartsWithNumberRegex();
}
