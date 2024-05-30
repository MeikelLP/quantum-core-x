using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;

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

    public async Task LoadAsync(CancellationToken token = default)
    {
        // todo: load skills from database
        _logger.LogInformation("Loaded {Count} skills", _skills.Length);
    }
}
