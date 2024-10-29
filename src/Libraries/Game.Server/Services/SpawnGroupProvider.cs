using QuantumCore.API.Game.World;
using QuantumCore.Game.Extensions;

namespace QuantumCore.Game.Services;

public class SpawnGroupProvider : ISpawnGroupProvider
{
    private readonly IParserService _parserService;

    public SpawnGroupProvider(IParserService parserService)
    {
        _parserService = parserService;
    }

    public async Task<IEnumerable<SpawnGroup>> GetSpawnGroupsAsync()
    {
        const string file = "data/group.txt";
        if (!File.Exists(file)) return Array.Empty<SpawnGroup>();

        using var sr = new StreamReader(file);

        var groups = await _parserService.ParseFileGroups(sr);

        var spawnGroups = groups.Select(g => g.ToSpawnGroup());

        return spawnGroups;
    }

    public async Task<IEnumerable<SpawnGroupCollection>> GetSpawnGroupCollectionsAsync()
    {
        const string file = "data/group_group.txt";
        if (!File.Exists(file)) return Array.Empty<SpawnGroupCollection>();

        using var sr = new StreamReader(file);

        var groups = await _parserService.ParseFileGroups(sr);

        var collections = groups.Select(x => x.ToSpawnGroupCollection());

        return collections;
    }
}