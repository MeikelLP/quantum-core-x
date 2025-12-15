using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.Guild;

namespace QuantumCore.Game.PlayerUtils;

public class GuildExperienceManager : IGuildExperienceManager
{
    private readonly ILogger<GuildExperienceManager> _logger;
    private readonly List<(uint Experience, ushort MaxPlayers)> _experienceTable = [];

    public byte MaxLevel => (byte) _experienceTable.Count;

    public GuildExperienceManager(ILogger<GuildExperienceManager> logger)
    {
        _logger = logger;
    }

    public uint GetNeededExperience(byte level)
    {
        return level > MaxLevel
            ? 0
            : _experienceTable[level - 1].Experience;
    }

    public ushort GetMaxPlayers(byte level)
    {
        return level > MaxLevel
            ? GuildConstants.MEMBERS_MAX
            : _experienceTable[level - 1].MaxPlayers;
    }

    public async Task LoadAsync(CancellationToken token = default)
    {
        _logger.LogInformation("Loading exp_guild.csv");
        var path = Path.Join("data", "exp_guild.csv");
        if (!File.Exists(path))
        {
            _logger.LogError("No guild experience table found!");
            return;
        }

        const char DELIMITER = ';';
        var lines = await File.ReadAllLinesAsync(path, token);
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
            {
                break;
            }

            var parts = line.Split(DELIMITER);

            if (!uint.TryParse(parts[0], out var experience))
            {
                _logger.LogError("Failed to parse experience table!. Invalid value on line {Line}: {Value}", i + 1,
                    parts[0]);
                _experienceTable.Clear();
                return;
            }

            if (!ushort.TryParse(parts[1], out var maxPlayers))
            {
                _logger.LogError("Failed to parse experience table!. Invalid value on line {Line}: {Value}", i + 1,
                    parts[1]);
                _experienceTable.Clear();
                return;
            }

            _experienceTable.Add((experience, maxPlayers));
        }
    }
}
