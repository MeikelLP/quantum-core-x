using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using EnumsNET;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types.Monsters;
using QuantumCore.API.Game.Types.Skills;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Drops;
using QuantumCore.Game.World;

namespace QuantumCore.Game.Services;

public partial class ParserService : IParserService
{
    private readonly IFileProvider _fileProvider;
    private static readonly NumberFormatInfo InvNum = NumberFormatInfo.InvariantInfo;
    private static readonly Encoding DefaultEncoding = Encoding.GetEncoding("EUC-KR");
    private const StringComparison InvCul = StringComparison.InvariantCultureIgnoreCase;

    private readonly ILogger<ParserService> _logger;

    public ParserService(ILoggerFactory loggerFactory, IFileProvider fileProvider)
    {
        _fileProvider = fileProvider;
        _logger = loggerFactory.CreateLogger<ParserService>();
    }


    public SpawnPoint? GetSpawnFromLine(string line)
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
            Direction = (ESpawnPointDirection)int.Parse(splitted[6]),
            RespawnTime = ParseSecondsFromTimespanString(splitted[7].Trim()),
            Chance = short.Parse(splitted[8]),
            MaxAmount = short.Parse(splitted[9]),
            Monster = uint.Parse(splitted[10])
        };
    }

    public async Task<ImmutableArray<CommonDropEntry>> GetCommonDropsAsync(TextReader sr,
        CancellationToken cancellationToken = default)
    {
        var list = new List<CommonDropEntry>();
        while (await sr.ReadLineAsync(cancellationToken) is { } line)
        {
            ParseCommonDropAndAdd(line, list);
        }

        return list.ToImmutableArray();
    }

    public async Task<ImmutableArray<SkillData>> GetSkillsAsync(string path, CancellationToken token = default)
    {
        var file = _fileProvider.GetFileInfo(path);
        if (!file.Exists)
        {
            _logger.LogWarning("{Path} does not exist, skills information not loaded", path);
            return [];
        }

        var list = new List<SkillData>();
        await using var fs = file.CreateReadStream();
        using var sr = new StreamReader(fs);
        while (!sr.EndOfStream)
        {
            var line = await sr.ReadLineAsync(token).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(line) || !StartsWithNumberRegex().IsMatch(line)) continue;

            // parse line
            var split = line.Split('\t');

            try
            {
                if (!Enum.TryParse<ESkill>(split[0], true, out var id))
                {
                    _logger.LogWarning("Failed to parse Skill with Id({Id}) from line: {Line}", split[0], line);
                    continue;
                }

                var data = new SkillData
                {
                    Id = id,
                    Name = split[1],
                    Type = (ESkillCategoryType)short.Parse(split[2]),
                    LevelStep = short.Parse(split[3]),
                    MaxLevel = short.Parse(split[4]),
                    LevelLimit = short.Parse(split[5]),
                    PointOn = split[6],
                    PointPoly = split[7],
                    SpCostPoly = split[8],
                    DurationPoly = split[9],
                    DurationSpCostPoly = split[10],
                    CooldownPoly = split[11],
                    MasterBonusPoly = split[12],
                    AttackGradePoly = split[13],
                    Flags = ExtractSkillFlags(split[14]),
                    AffectFlag = ExtractAffectFlags(split[15]),
                    PointOn2 = split[16],
                    PointPoly2 = split[17],
                    DurationPoly2 = split[18],
                    AffectFlag2 = ExtractAffectFlags(split[19]),
                    PrerequisiteSkillVnum = int.Parse(split[20]),
                    PrerequisiteSkillLevel = int.Parse(split[21]),
                    SkillType =
                        Enum.TryParse<ESkillType>(split[22], true, out var result) ? result : ESkillType.Normal,
                    MaxHit = short.Parse(split[23]),
                    SplashAroundDamageAdjustPoly = split[24],
                    TargetRange = int.Parse(split[25]),
                    SplashRange = uint.Parse(split[26])
                };

                list.Add(data);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to parse skill data from line: {Line}", line);
            }
        }

        _logger.LogInformation("Loaded {Count} skills", list.Count);

        return [..list];
    }

    public async Task<List<DataFileGroup>> ParseFileGroups(StreamReader sr, CancellationToken token = default)
    {
        var groups = new List<DataFileGroup>();
        DataFileGroup? currentGroup = null;

        while (await sr.ReadLineAsync(token) is { } line && sr.EndOfStream == false)
        {
            if (line.Trim().All(c => c == '\t') || string.IsNullOrWhiteSpace(line.Trim()))
            {
                continue;
            }

            if (line.StartsWith("Group", InvCul))
            {
                line = line.Trim();
                if (currentGroup != null)
                {
                    groups.Add(currentGroup);
                }

                // remove empty or whitespace entries
                line = SplitByWhitespaceOrTabRegex().Replace(line, " ");
                currentGroup = new DataFileGroup {Name = line.Split()[1]};
            }
            else if (!string.IsNullOrWhiteSpace(line) && currentGroup != null)
            {
                var parts = SplitByWhitespaceOrTabRegex().Split(line).ToList();

                parts.RemoveAll(IsEmptyOrContainsNewlineOrTab);

                if (parts.Count == 0) continue; // can happen due to filtering

                for (var i = 0; i < parts.Count; i++)
                {
                    parts[i] = parts[i].Trim();
                }

                if (!StartsWithNumberRegex().IsMatch(parts[0])) // Assuming all fields do not start with a number
                {
                    currentGroup.Fields[parts[0].Trim()] = parts[^1].Trim();
                }
                else
                {
                    if (parts.Count == 0) continue; // can happen due to filtering
                    currentGroup.Data.Add(parts);
                }
            }
        }

        if (currentGroup != null)
        {
            groups.Add(currentGroup);
        }

        return groups;
    }

    public MonsterDropContainer? ParseMobGroup(DataFileGroup group, IItemManager itemManager)
    {
        uint minKillCount = 0;
        uint levelLimit = 0;

        var type = group.GetField<string>("Type");
        if (type == default)
        {
            throw new MissingRequiredFieldException("Type");
        }

        var monsterProtoId = group.GetField<uint>("Mob");
        if (monsterProtoId == default)
        {
            throw new MissingRequiredFieldException("Mob");
        }

        if (type.Equals("Kill", InvCul))
        {
            minKillCount = group.GetField<uint>("kill_drop");
        }
        else
        {
            minKillCount = 1;
        }

        if (type.Equals("Limit", InvCul))
        {
            levelLimit = group.GetField<uint>("Level_limit");
            if (levelLimit == default)
            {
                throw new MissingRequiredFieldException("Level_limit");
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

        if (type.Equals("Kill", InvCul)) // MobItemGroup
        {
            var entry = new MonsterItemGroup {MonsterProtoId = monsterProtoId, MinKillCount = minKillCount,};

            foreach (var dropData in group.Data)
            {
                var itemProtoId = uint.TryParse(dropData[1], InvNum, out var id) ? id : 0;
                if (itemProtoId < 1)
                {
                    var item = itemManager.GetItemByName(dropData[1]); // Some entries are the names instead of the id
                    if (item == null)
                    {
                        throw new MissingRequiredFieldException("ItemProtoId");
                    }

                    itemProtoId = item.Id;
                }

                var count = uint.Parse(dropData[2], InvNum);
                if (count < 1)
                {
                    throw new MissingRequiredFieldException("Count");
                }

                var chance = uint.TryParse(dropData[3], InvNum, out var ch) ? ch : 0;
                if (chance <= 0)
                {
                    throw new MissingRequiredFieldException("Chance");
                }

                var rareChance = int.Parse(dropData[4], InvNum);
                rareChance = MathUtils.MinMax(0, rareChance, 100);

                entry.AddDrop(itemProtoId, count, chance, (uint)rareChance);
            }

            return entry;
        }

        if (type.Equals("Drop", InvCul)) // DropItemGroup
        {
            var entry = new DropItemGroup {MonsterProtoId = monsterProtoId,};

            foreach (var dropData in group.Data)
            {
                var itemProtoId = uint.Parse(dropData[1], InvNum);
                if (itemProtoId < 1)
                {
                    throw new MissingRequiredFieldException("ItemProtoId");
                }

                var count = uint.Parse(dropData[2], InvNum);
                if (count < 1)
                {
                    throw new MissingRequiredFieldException("Count");
                }

                var chance = float.Parse(dropData[3], InvNum);
                if (chance <= 0)
                {
                    throw new MissingRequiredFieldException("Chance");
                }

                chance *= 10000.0f; // to make it 0-1000

                entry.Drops.Add(new DropItemGroup.Drop {ItemProtoId = itemProtoId, Amount = count, Chance = chance});
            }

            return entry;
        }

        if (type.Equals("Limit", InvCul)) // LevelItemGroup
        {
            var entry = new LevelItemGroup {LevelLimit = levelLimit};

            foreach (var dropData in group.Data)
            {
                uint itemProtoId = uint.TryParse(dropData[1], InvNum, out var id) ? id : 0;
                if (itemProtoId < 1)
                {
                    var item = itemManager.GetItemByName(dropData[1]); // Some entries are the names instead of the id
                    if (item == null)
                    {
                        throw new MissingRequiredFieldException("ItemProtoId");
                    }

                    itemProtoId = item.Id;
                }

                var count = uint.Parse(dropData[2], InvNum);
                if (count < 1)
                {
                    throw new MissingRequiredFieldException("Count");
                }

                var chance = float.Parse(dropData[3], InvNum);
                if (chance <= 0)
                {
                    throw new MissingRequiredFieldException("Chance");
                }

                chance *= 10000.0f; // to make it 0-1000

                entry.Drops.Add(new LevelItemGroup.Drop {ItemProtoId = itemProtoId, Amount = count, Chance = chance});
            }

            return entry;
        }

        return null;
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

    private static ESkillFlags ExtractSkillFlags(string value)
    {
        ESkillFlags result = 0;

        if (string.IsNullOrWhiteSpace(value))
        {
            return result;
        }

        var flags = value.Split(',');

        foreach (var flag in flags)
        {
            if (!EnumUtils.TryParseEnum<ESkillFlags>(flag, out var parsed)) continue;
            result |= parsed;
        }

        return result;
    }

    private static EAffectFlags ExtractAffectFlags(string value)
    {
        EAffectFlags result = 0;

        if (string.IsNullOrWhiteSpace(value))
        {
            return result;
        }

        var flags = value.Split(',');

        foreach (var flag in flags)
        {
            if (!EnumUtils.TryParseEnum<EAffectFlags>(flag, out var parsed)) continue;
            result |= parsed;
        }

        return result;
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
        var percentage =
            float.Parse(line[percentageStartIndex..percentageEndIndex],
                CultureInfo.InvariantCulture); // math percentage
        var itemId = uint.Parse(line[itemIdStartIndex..itemIdEndIndex]);
        var outOf = uint.Parse(
            line[outOfStartIndex..outOfEndIndex]); // TODO: what to do with this value? Doesnt seem to be used, needs confirmation

        read = outOfEndIndex + 1;

        percentage *= 10000.0f; // because percentage here is 1 - 1000

        return new CommonDropEntry(minLevel, maxLevel, itemId, percentage);
    }

    private static bool IsEmptyOrContainsNewlineOrTab(string str)
    {
        return string.IsNullOrEmpty(str)
               || str.Contains("\n")
               || str.Contains("\t")
               || str.Contains("{")
               || str.Contains("}");
    }

    [DebuggerDisplay("{Name} | {Fields.Count} - {Data.Count}")]
    public class DataFileGroup
    {
        public string Name { get; set; }
        public Dictionary<string, string> Fields { get; } = new(StringComparer.InvariantCultureIgnoreCase);
        public List<List<string>> Data { get; } = new();

        public T? GetField<T>(string key)
        {
            var foundKey = Fields.Keys.FirstOrDefault(k => k.Equals(key, StringComparison.InvariantCultureIgnoreCase));
            if (foundKey == null)
            {
                return default;
            }

            var value = Fields[foundKey];
            return (T)Convert.ChangeType(value, typeof(T));
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

    [GeneratedRegex("(?: {2,}|\\t+)")]
    private static partial Regex SplitByWhitespaceRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex SplitByWhitespaceOrTabRegex();

    [GeneratedRegex(@"^\d")]
    private static partial Regex StartsWithNumberRegex();
}
