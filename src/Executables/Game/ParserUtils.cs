using System.Text.RegularExpressions;
using EnumsNET;
using QuantumCore.API.Game.World;
using QuantumCore.Game.World;

namespace QuantumCore.Game;

internal static partial class ParserUtils
{
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

    public static string[] SplitLine(string line)
    {
        if (line.Contains('\t'))
        {
            return line.Split('\t');
        }

        return line.Split(' ');
    }

    [GeneratedRegex("(?: {2,}|\\t+)")]
    private static partial Regex SplitByWhitespaceRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex SplitByWhitespaceOrTabRegex();

    [GeneratedRegex("Leader")]
    private static partial Regex LeaderReplaceRegex();

    [GeneratedRegex("Group")]
    private static partial Regex GroupReplaceRegex();
}
