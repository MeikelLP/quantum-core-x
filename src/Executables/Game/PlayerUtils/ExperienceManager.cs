using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace QuantumCore.Game.PlayerUtils;

public class ExperienceManager : IExperienceManager
{
    private readonly ILogger<ExperienceManager> _logger;
    private readonly List<uint> _experienceTable = new();

    public byte MaxLevel => (byte) _experienceTable.Count;

    public uint GetNeededExperience(byte level)
    {
        Debug.Assert(level > 0);
        return level > MaxLevel ? 0 : _experienceTable[level - 1];
    }

    public ExperienceManager(ILogger<ExperienceManager> logger)
    {
        _logger = logger;
    }

    public async Task LoadAsync(CancellationToken token = default)
    {
        _logger.LogInformation("Loading exp.csv");
        var path = Path.Join("data", "exp.csv");
        if (!File.Exists(path))
        {
            _logger.LogError("No experience table found!");
            return;
        }

        var lines = await File.ReadAllLinesAsync(path, token);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                break;
            }

            if (!uint.TryParse(line, out var experience))
            {
                _logger.LogError("Failed to parse experience table!");
                _experienceTable.Clear();
                return;
            }
            
            _experienceTable.Add(experience);
        }
    }
}