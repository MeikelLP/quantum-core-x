using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using QuantumCore.Game.World;

namespace QuantumCore.Game.Services;

internal class SpawnPointProvider : ISpawnPointProvider
{
    private readonly ILogger<SpawnPointProvider> _logger;
    private readonly IParserService _parserService;
    private readonly IFileProvider _fileProvider;

    public SpawnPointProvider(ILogger<SpawnPointProvider> logger, IParserService parserService,
        IFileProvider fileProvider)
    {
        _logger = logger;
        _parserService = parserService;
        _fileProvider = fileProvider;
    }

    public async Task<SpawnPoint[]> GetSpawnPointsForMap(string name)
    {
        var list = new List<SpawnPoint>();

        _logger.LogDebug("Loading spawn points for map {Map}", name);

        await AddSpawnPointsFromFile($"maps/{name}/regen.txt", list);
        await AddSpawnPointsFromFile($"maps/{name}/npc.txt", list);
        await AddSpawnPointsFromFile($"maps/{name}/stone.txt", list);
        await AddSpawnPointsFromFile($"maps/{name}/boss.txt", list);

        _logger.LogDebug("Found {Count:D} spawn points for map {Map}", list.Count, name);

        return list.ToArray();
    }

    private async Task AddSpawnPointsFromFile(string filePath, List<SpawnPoint> list)
    {
        var file = _fileProvider.GetFileInfo(filePath);
        if (!file.Exists) return;

        await using var fs = file.CreateReadStream();
        using var sr = new StreamReader(fs);
        do
        {
            var line = await sr.ReadLineAsync();
            if (line is null) break;

            var spawn = _parserService.GetSpawnFromLine(line);
            if (spawn is not null)
            {
                list.Add(spawn);
            }
        } while (!sr.EndOfStream);
    }
}