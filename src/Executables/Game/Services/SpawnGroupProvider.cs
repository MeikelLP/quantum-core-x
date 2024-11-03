using Microsoft.Extensions.FileProviders;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Extensions;

namespace QuantumCore.Game.Services;

public class SpawnGroupProvider : ISpawnGroupProvider
{
    private readonly IParserService _parserService;
    private readonly IFileProvider _fileProvider;

    public SpawnGroupProvider(IParserService parserService, IFileProvider fileProvider)
    {
        _parserService = parserService;
        _fileProvider = fileProvider;
    }

    public async Task<IEnumerable<SpawnGroup>> GetSpawnGroupsAsync()
    {
        var file = _fileProvider.GetFileInfo("group.txt");
        if (!file.Exists) return Array.Empty<SpawnGroup>();

        await using var fs = file.CreateReadStream();
        using var sr = new StreamReader(fs);

        var groups = await _parserService.ParseFileGroups(sr);

        var spawnGroups = groups.Select(g => g.ToSpawnGroup());

        return spawnGroups;
    }

    public async Task<IEnumerable<SpawnGroupCollection>> GetSpawnGroupCollectionsAsync()
    {
        var file = _fileProvider.GetFileInfo("group_group.txt");
        if (!file.Exists) return Array.Empty<SpawnGroupCollection>();

        await using var fs = file.CreateReadStream();
        using var sr = new StreamReader(fs);

        var groups = await _parserService.ParseFileGroups(sr);

        var collections = groups.Select(x => x.ToSpawnGroupCollection());

        return collections;
    }
}
