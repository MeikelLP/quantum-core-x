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
                           splitted[0].AsSpan()[1..2].Equals("a", INV_CUL),
            X = int.Parse(splitted[1]),
            Y = int.Parse(splitted[2]),
            RangeX = int.Parse(splitted[3]),
            RangeY = int.Parse(splitted[4]),
            Direction = int.Parse(splitted[5]),
            // splitted[6],
            RespawnTime = ParseSecondsFromTimespanString(splitted[7]),
            Chance = short.Parse(splitted[8]),
            MaxAmount = short.Parse(splitted[9]),
            Monster = uint.Parse(splitted[10])
        };
    }

    private static int ParseSecondsFromTimespanString(string str)
    {
        var value = int.Parse(str.AsSpan()[..^1]);
        if (str.EndsWith("s", INV_CUL))
        {
            return value;
        }

        if (str.EndsWith("m", INV_CUL))
        {
            return value * 60;
        }

        if (str.EndsWith("h", INV_CUL))
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
        item.Leader = uint.Parse(SplitByWhitespaceRegex().Split(LeaderReplaceRegex().Replace(line, "", 1).Trim())[1]
            .Trim());
        while ((line = (await sr.ReadLineAsync())!.Trim()) != "}")
        {
            if (string.IsNullOrWhiteSpace(line)) break;
            item.Members.Add(new SpawnMember
            {
                Id = uint.Parse(SplitByWhitespaceRegex().Split(line)[^1].Trim())
            });
        }

        return item;
    }

    /// <returns>null if end of stream</returns>
    public static async Task<KeyValuePair<uint, DropEntry[]>?> GetDropsForBlockAsync(TextReader sr,
        CancellationToken cancellationToken = default)
    {
        string? line;
        do
        {
            line = await sr.ReadLineAsync(cancellationToken);

            if (line is null) return null; // EOS
        } while (!line.StartsWith("Group"));

        var drops = new List<DropEntry>();
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
                    drops.Add(new DropEntry(itemId, chance, minLevel, minKillCount, amount));
                }
                else
                {
                    // invalid item Id -- cannot handle -- will be ignored
                }
            }
        }

        if (mob == 0) return null; // invalid

        return new KeyValuePair<uint, DropEntry[]>(mob, drops.ToArray());
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
            item.Groups.Add(new SpawnGroupCollectionMember
            {
                Id = uint.Parse(splitted[^2].Trim()),
                Amount = byte.Parse(splitted[^1].Trim())
            });
        }

        return item;
    }

    [GeneratedRegex("(?: {2,}|\\t+)")]
    private static partial Regex SplitByWhitespaceRegex();

    [GeneratedRegex("Leader")]
    private static partial Regex LeaderReplaceRegex();

    [GeneratedRegex("Group")]
    private static partial Regex GroupReplaceRegex();

    [GeneratedRegex(@"^\d")]
    private static partial Regex StartsWithNumberRegex();
}
