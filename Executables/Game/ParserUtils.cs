using System.Text.RegularExpressions;
using EnumsNET;
using QuantumCore.API.Game.World;
using QuantumCore.Game.World;

namespace QuantumCore.Game;

internal static partial class ParserUtils
{
    public static SpawnPoint GetSpawnFromLine(string line)
    {
        // r	751	311	10	10	0	0	5s	100	1	101
        var splitted = SplitByWhitespaceRegex().Split(line);
        return new SpawnPoint
        {
            Type = Enums.Parse<ESpawnPointType>(splitted[0][..1], true, EnumFormat.EnumMemberValue),
            IsAggressive = splitted[0].Length > 1 && splitted[0][1..2].Equals("a", StringComparison.InvariantCultureIgnoreCase),
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
        item.Leader = uint.Parse(SplitByWhitespaceRegex().Split(LeaderReplaceRegex().Replace(line, "", 1).Trim())[1].Trim());
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
}
