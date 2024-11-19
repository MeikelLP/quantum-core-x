using System.Diagnostics;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using QuantumCore.API;

namespace QuantumCore.Game.PlayerUtils;

public class ExperienceManager : IExperienceManager, ILoadable
{
    private readonly ILogger<ExperienceManager> _logger;
    private readonly IFileProvider _fileProvider;
    private readonly List<uint> _experienceTable = new();

    public byte MaxLevel => (byte) _experienceTable.Count;

    public uint GetNeededExperience(byte level)
    {
        Debug.Assert(level > 0);
        return level > MaxLevel ? 0 : _experienceTable[level - 1];
    }

    public ExperienceManager(ILogger<ExperienceManager> logger, IFileProvider fileProvider)
    {
        _logger = logger;
        _fileProvider = fileProvider;
    }

    public async Task LoadAsync(CancellationToken token = default)
    {
        var file = _fileProvider.GetFileInfo("exp.csv");

        if (!file.Exists)
        {
            _logger.LogWarning("{Path} does not exist, experience table not loaded", file.PhysicalPath);
            return;
        }

        await using var fs = file.CreateReadStream();
        using var sr = new StreamReader(fs);
        var i = 0;
        while (!sr.EndOfStream)
        {
            var line = await sr.ReadLineAsync(token).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(line))
            {
                break;
            }

            if (!uint.TryParse(line, out var experience))
            {
                _logger.LogError("Failed to parse experience table!. Invalid value on line {Line}: {Value}", i + 1,
                    line);
                _experienceTable.Clear();
                return;
            }

            _experienceTable.Add(experience);
            i++;
        }
    }
}
