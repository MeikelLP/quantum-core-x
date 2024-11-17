using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Skills;
using QuantumCore.Game.Services;

namespace QuantumCore.Game;

/// <summary>
/// Manage all static data related to skills
/// </summary>
public class SkillManager : ISkillManager, ILoadable
{
    private readonly ILogger<SkillManager> _logger;
    private readonly IParserService _parserService;
    private ImmutableArray<SkillData> _skills = ImmutableArray<SkillData>.Empty;

    public SkillManager(ILogger<SkillManager> logger, IParserService parserService)
    {
        _logger = logger;
        _parserService = parserService;
    }

    public SkillData? GetSkill(ESkillIndexes id)
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
        if (_skills.Length > 0)
        {
            return;
        }

        _skills = await _parserService.GetSkillsAsync("skilltable.txt", token);
    }

    public Task ReloadAsync(CancellationToken token = default)
    {
        _skills = _skills.Clear();
        return LoadAsync(token);
    }
}
