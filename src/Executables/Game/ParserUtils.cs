using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using EnumsNET;
using QuantumCore.API;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Drops;
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
        //todo: each line has 4 sections (1 section for each mob rank), currently only parsing the first one
        
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
        var percentage = float.Parse(line[percentageStartIndex..percentageEndIndex], CultureInfo.InvariantCulture); // math percentage
        var itemId = uint.Parse(line[itemIdStartIndex..itemIdEndIndex]);
        var outOf = uint.Parse(line[outOfStartIndex..outOfEndIndex]); // TODO: what to do with this value? Doesnt seem to be used, needs confirmation

        read = outOfEndIndex + 1;
        
        percentage *= 10000.0f; // because percentage here is 1 - 1000
        
        return new CommonDropEntry(minLevel, maxLevel, itemId, percentage);
    }

    public static string[] SplitLine(string line)
    {
        if (line.Contains('\t'))
        {
            return line.Split('\t', StringSplitOptions.RemoveEmptyEntries);
        }

        return line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }
    
    [DebuggerDisplay("{Fields.Count} Fields - {Data.Count} Drops")]
    private class MobDropGroup
    {
        public string Name { get; set; }
        public Dictionary<string, string> Fields { get; } = new(StringComparer.InvariantCultureIgnoreCase);
        public List<List<string>> Data { get; } = new List<List<string>>();
        
        public T? GetField<T>(string key)
        {
            var foundKey = Fields.Keys.FirstOrDefault(k => k.Equals(key, StringComparison.InvariantCultureIgnoreCase));
            if (foundKey == null)
            {
                return default;
            }
            var value = Fields[foundKey];
            return (T) Convert.ChangeType(value, typeof(T));
        }

        public override string ToString()
        {
            var result = $"Group {Name}\n{{\n";
            foreach (var field in Fields)
            {
                result += $"\t{field.Key}\t{field.Value}\n";
            }
            foreach (var datum in Data)
            {
                result += $"\t{string.Join("\t", datum)}\n";
            }
            result += "}\n";
            return result;
        }
    }
    
    public static IEnumerable<MonsterDropContainer> GetDropsForGroupBlocks(string filePath,
        Encoding encoding, IItemManager itemManager)
    {
        var groups = new List<MobDropGroup>();
        MobDropGroup? currentMobDropGroup = null;

        foreach (var line in File.ReadLines(filePath, encoding))
        {
            if (line.Trim().All(c => c == '\t') || string.IsNullOrWhiteSpace(line.Trim()))
            {
                continue;
            }
            if (line.StartsWith("Group", INV_CUL))
            {
                if (currentMobDropGroup != null)
                {
                    groups.Add(currentMobDropGroup);
                }
                currentMobDropGroup = new MobDropGroup { Name = line.Split()[1] };
            }
            else if (!string.IsNullOrWhiteSpace(line) && currentMobDropGroup != null)
            {
                var parts = line.Split('\t').ToList();
                
                parts.RemoveAll(IsEmptyOrContainsNewlineOrTab);
                
                if (parts.Count == 2)
                {
                    currentMobDropGroup.Fields[parts[0].Trim()] = parts[1].Trim();
                }
                else
                {
                    if (parts.Count == 0) continue; // can happen due to filtering
                    parts.ForEach(p => p.Trim());
                    currentMobDropGroup.Data.Add(parts);
                }
            }
        }

        if (currentMobDropGroup != null)
        {
            if (currentMobDropGroup.Fields.Keys.Any(k => StartsWithNumberRegex().IsMatch(k)))
            {
                throw new Exception("Invalid group format");
            }
            groups.Add(currentMobDropGroup);
        }
        
        foreach (var group in groups)
        {
            var container = ParseMobGroup(group, itemManager);
            if (container != null)
            {
                yield return container;
            }
        }
    }
    
    private static bool IsEmptyOrContainsNewlineOrTab(string str)
    {
        return string.IsNullOrEmpty(str) || str.Contains("\n") || str.Contains("\t") || str.Contains("{") || str.Contains("}");
    }

    private static MonsterDropContainer? ParseMobGroup(MobDropGroup group, IItemManager itemManager)
    {
        uint minKillCount = 0;
        uint levelLimit = 0;
        
        var type = group.GetField<string>("Type");
        if (type == default)
        {
            // todo: custom exceptions
            throw new InvalidOperationException("Type not found in group");
        }
        
        var mobId = group.GetField<uint>("Mob");
        if (mobId == default)
        {
            throw new InvalidOperationException("Mob not found in group");
        }

        if (type.Equals("Kill", INV_CUL))
        {
            minKillCount = group.GetField<uint>("kill_drop");
        }
        else
        {
            minKillCount = 1;
        }
        
        if (type.Equals("Limit", INV_CUL))
        {
            levelLimit = group.GetField<uint>("Level_limit");
            if (levelLimit == default)
            {
                throw new InvalidOperationException("Level_limit not found in group when type is limit");
            }
        }
        else
        {
            levelLimit = 0;
        }

        if (minKillCount == 0)
        {
            return null;
        }

        if (type.Equals("Kill", INV_CUL)) // MobItemGroup
        {
            var entry = new MonsterItemGroup
            {
                MonsterProtoId = mobId,
                MinKillCount = minKillCount,
            };

            foreach (var dropData in group.Data)
            {
                uint itemProtoId = uint.TryParse(dropData[1], InvNum, out var id) ? id : 0;
                if (itemProtoId < 1)
                {
                    var item = itemManager.GetItemByName(dropData[1]); // Some entries are the names instead of the id
                    if (item == null)
                    {
                        throw new InvalidOperationException("Invalid item proto id");
                    }
                    itemProtoId = item.Id;
                }
                
                uint count = uint.Parse(dropData[2], InvNum);
                if (count < 1)
                {
                    throw new InvalidOperationException("Invalid count");
                }
                
                uint chance = uint.Parse(dropData[3], InvNum);
                if (chance <= 0)
                {
                    throw new InvalidOperationException("Invalid chance");
                }
                
                int rareChance = int.Parse(dropData[4], InvNum);
                rareChance = MathUtils.MinMax(0, rareChance, 100);
                
                entry.AddDrop(itemProtoId, count, chance, (uint) rareChance);
            }
            
            return entry;
            
        }

        if (type.Equals("Drop", INV_CUL)) // DropItemGroup
        {
            var entry = new DropItemGroup
            {
                MonsterProtoId = mobId,
            };

            foreach (var dropData in group.Data)
            {
                uint itemProtoId = uint.Parse(dropData[1], InvNum);
                if (itemProtoId < 1)
                {
                    throw new InvalidOperationException("Invalid item id");
                }
                
                uint count = uint.Parse(dropData[2], InvNum);
                if (count < 1)
                {
                    throw new InvalidOperationException("Invalid count");
                }
                
                float chance = float.Parse(dropData[3], InvNum);
                if (chance <= 0)
                {
                    throw new InvalidOperationException("Invalid chance");
                }
                
                chance *= 10000.0f; // to make it 0-1000
                
                entry.Drops.Add(new DropItemGroup.Drop { ItemProtoId = itemProtoId, Amount = count, Chance = chance });
            }
            
            return entry;
        }

        if (type.Equals("Limit", INV_CUL)) // LevelItemGroup
        {
            var entry = new LevelItemGroup
            {
                LevelLimit = levelLimit
            };
            
            foreach (var dropData in group.Data)
            {
                uint itemProtoId = uint.TryParse(dropData[1], InvNum, out var id) ? id : 0;
                if (itemProtoId < 1)
                {
                    var item = itemManager.GetItemByName(dropData[1]); // Some entries are the names instead of the id
                    if (item == null)
                    {
                        throw new InvalidOperationException("Invalid item proto id");
                    }
                    itemProtoId = item.Id;
                }
                
                uint count = uint.Parse(dropData[2], InvNum);
                if (count < 1)
                {
                    throw new InvalidOperationException("Invalid count");
                }
                
                float chance = float.Parse(dropData[3], InvNum);
                if (chance <= 0)
                {
                    throw new InvalidOperationException("Invalid chance");
                }
                
                chance *= 10000.0f; // to make it 0-1000
                
                entry.Drops.Add(new LevelItemGroup.Drop { ItemProtoId = itemProtoId, Amount = count, Chance = chance });
            }
            
            return entry;
        }
        
        return null;
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
