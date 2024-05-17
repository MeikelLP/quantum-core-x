﻿using Microsoft.Extensions.Logging;
using QuantumCore.Game.World;

namespace QuantumCore.Game.Services;

internal class SpawnPointProvider : ISpawnPointProvider
{
    private readonly ILogger<SpawnPointProvider> _logger;

    public SpawnPointProvider(ILogger<SpawnPointProvider> logger)
    {
        _logger = logger;
    }

    public async Task<SpawnPoint[]> GetSpawnPointsForMap(string name)
    {
        var list = new List<SpawnPoint>();

        _logger.LogDebug("Loading spawn points for map {Map}", name);

        await AddSpawnPointsFromFile($"data/maps/{name}/regen.txt", list);
        await AddSpawnPointsFromFile($"data/maps/{name}/npc.txt", list);
        await AddSpawnPointsFromFile($"data/maps/{name}/stone.txt", list);
        await AddSpawnPointsFromFile($"data/maps/{name}/boss.txt", list);

        _logger.LogDebug("Found {Count:D} spawn points for map {Map}", list.Count, name);

        return list.ToArray();
    }

    private static async Task AddSpawnPointsFromFile(string filePath, List<SpawnPoint> list)
    {
        if (!File.Exists(filePath)) return;

        using var sr = new StreamReader(filePath);
        do
        {
            var line = await sr.ReadLineAsync();
            if (line is null) break;

            var spawn = Game.ParserUtils.GetSpawnFromLine(line);
            if (spawn is not null)
            {
                list.Add(spawn);
            }
        } while (!sr.EndOfStream);
    }
}
