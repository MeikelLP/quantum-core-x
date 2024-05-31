using System.Collections.Immutable;
using System.Text;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Skills;

namespace QuantumCore.Game;

/// <summary>
/// Manage all static data related to skills
/// </summary>
public class SkillManager : ISkillManager
{
    private readonly ILogger<SkillManager> _logger;
    private ImmutableArray<SkillData> _skills = ImmutableArray<SkillData>.Empty;
    
    public SkillManager(ILogger<SkillManager> logger)
    {
        _logger = logger;
    }
    
    public SkillData? GetSkill(uint id)
    {
        return _skills.FirstOrDefault(skill => skill.Id == id);
    }

    public SkillData? GetSkillByName(ReadOnlySpan<char> name)
    {
        foreach (var dataSkill in _skills)
        {
            if (name.Equals(dataSkill.Name, StringComparison.InvariantCulture))
            {
                return dataSkill;
            }
        }
        return null;
    }
    
    public static bool TryParseEnum<TEnum>(string value, out TEnum result) where TEnum : struct, Enum
    {
        // Check if the value is a single word
        if (!value.Contains("_"))
        {
            // Capitalize the first letter and lowercase the rest to match enum naming convention
            var formattedSingleWord = char.ToUpper(value[0]) + value.Substring(1).ToLower();
            return Enum.TryParse(formattedSingleWord, true, out result);
        }

        // If the value contains underscores, split and format it
        var parts = value.ToLower().Split('_');
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
        }

        var formattedValue = string.Join("", parts);

        // Try to parse the formatted string as the enum type
        return Enum.TryParse(formattedValue, true, out result);
    }

    public async Task LoadAsync(CancellationToken token = default)
    {
        if (_skills.Length > 0)
        {
            return;
        }
        
        var fileEncoding = Encoding.GetEncoding("EUC-KR");

        await foreach(var line in File.ReadLinesAsync("data/936skilltable.txt", fileEncoding, token))
        {
            // parse line
            var split = line.Split('\t');

            List<ESkillFlag> ExtractSkillFlags(string value)
            {
                var values = string.IsNullOrWhiteSpace(value) 
                    ? [] 
                    : value.Split(',').Select(flag => TryParseEnum<ESkillFlag>(flag, out var result) ? result : ESkillFlag.None).ToList();
                values.RemoveAll(v => v == ESkillFlag.None);
                return values;
            }
            
            List<ESkillAffectFlag> ExtractAffectFlags(string value)
            {
                return string.IsNullOrWhiteSpace(value) 
                    ? [ESkillAffectFlag.Ymir] 
                    : value.Split(',').Select(flag => TryParseEnum<ESkillAffectFlag>(flag, out var result) ? result : ESkillAffectFlag.Ymir).ToList();
            }
            
            try
            {
                var flags = ExtractSkillFlags(split[14]);
                var affectFlags = ExtractAffectFlags(split[15]);
                var affectFlags2 = ExtractAffectFlags(split[19]);
                
                var data = new SkillData
                {
                    Id = uint.Parse(split[0]),
                    Name = split[1],
                    Type = short.Parse(split[2]),
                    LevelStep = short.Parse(split[3]),
                    MaxLevel = short.Parse(split[4]),
                    LevelLimit = short.Parse(split[5]),
                    PointOn = split[6],
                    PointPoly = split[7],
                    SPCostPoly = split[8],
                    DurationPoly = split[9],
                    DurationSPCostPoly = split[10],
                    CooldownPoly = split[11],
                    MasterBonusPoly = split[12],
                    AttackGradePoly = split[13],
                    Flags = flags,
                    AffectFlags = affectFlags,
                    PointOn2 = split[16],
                    PointPoly2 = split[17],
                    DurationPoly2 = split[18],
                    AffectFlags2 = affectFlags2,
                    PrerequisiteSkillVnum = int.Parse(split[20]),
                    PrerequisiteSkillLevel = int.Parse(split[21]),
                    SkillType = Enum.TryParse<ESkillType>(split[22], true, out var result) ? result : ESkillType.Normal,
                    MaxHit = short.Parse(split[23]),
                    SplashAroundDamageAdjustPoly = split[24],
                    TargetRange = int.Parse(split[25]),
                    SplashRange = uint.Parse(split[26])
                };
                
                _skills = _skills.Add(data);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to parse skill line: {Line}", line);
                throw;
            }
        }
        
        _logger.LogInformation("Loaded {Count} skills", _skills.Length);
    }

    public Task ReloadAsync(CancellationToken token = default)
    {
        _skills = _skills.Clear();
        return LoadAsync(token);
    }
}
