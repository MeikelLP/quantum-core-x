using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.World;
using QuantumCore.Caching;
using QuantumCore.Game.World;

namespace QuantumCore.Game.Services;

internal partial class AtlasProvider : IAtlasProvider
{
    private record AtlasValue(string MapName, Coordinates Position, uint Width, uint Height);

    private static readonly AtlasValue[] DefaultAtlasValues =
    [
        new("metin2_map_a1", new Coordinates(409600, 896000), 4, 5),
        new("metin2_map_b1", new Coordinates(0, 102400), 4, 5),
        new("metin2_map_a1", new Coordinates(921600, 204800), 4, 5),
    ];

    private readonly IConfiguration _configuration;
    private readonly IMonsterManager _monsterManager;
    private readonly IAnimationManager _animationManager;
    private readonly ISpawnPointProvider _spawnPointProvider;
    private readonly ICacheManager _cacheManager;
    private readonly ILogger<AtlasProvider> _logger;
    private readonly IItemManager _itemManager;
    private readonly IFileProvider _fileProvider;
    private readonly IServerBase _server;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMapAttributeProvider _attributeProvider;

    /// <summary>
    /// Regex for parsing lines in the atlas info
    /// </summary>
    [GeneratedRegex(@"^([a-zA-Z0-9\/_]+)[\s]+([0-9]+)[\s]+([0-9]+)[\s]+([0-9]+)[\s]+([0-9]+)$", RegexOptions.Compiled,
        100)]
    private static partial Regex LineParser();

    public AtlasProvider(IConfiguration configuration, IMonsterManager monsterManager,
        IAnimationManager animationManager, ISpawnPointProvider spawnPointProvider,
        ICacheManager cacheManager, ILogger<AtlasProvider> logger, IItemManager itemManager,
        IFileProvider fileProvider, IServerBase server, IServiceProvider serviceProvider,
        IMapAttributeProvider attributeProvider)
    {
        _configuration = configuration;
        _monsterManager = monsterManager;
        _animationManager = animationManager;
        _spawnPointProvider = spawnPointProvider;
        _cacheManager = cacheManager;
        _logger = logger;
        _itemManager = itemManager;
        _fileProvider = fileProvider;
        _server = server;
        _serviceProvider = serviceProvider;
        _attributeProvider = attributeProvider;
    }

    public async Task<IEnumerable<IMap>> GetAsync(IWorld world)
    {
        var maxX = 0u;
        var maxY = 0u;

        var atlasValues = new List<AtlasValue>();
        var fileInfo = _fileProvider.GetFileInfo("atlasinfo.txt");
        if (!fileInfo.Exists)
        {
            atlasValues = [..DefaultAtlasValues];
            _logger.LogWarning("Not atlasinfo.txt found. Using default values.");
        }
        else
        {
            // Load atlasinfo.txt and initialize all maps the game core hosts
            await using var fs = fileInfo.CreateReadStream();
            using var reader = new StreamReader(fs);
            var lineNo = 0;
            while ((await reader.ReadLineAsync())?.Trim()! is { } line)
            {
                lineNo++;
                if (string.IsNullOrWhiteSpace(line)) continue; // skip empty lines

                var match = LineParser().Match(line);
                if (match.Success)
                {
                    try
                    {
                        var mapName = match.Groups[1].Value;
                        var positionX = uint.Parse(match.Groups[2].Value);
                        var positionY = uint.Parse(match.Groups[3].Value);
                        var width = uint.Parse(match.Groups[4].Value);
                        var height = uint.Parse(match.Groups[5].Value);
                        atlasValues.Add(new AtlasValue(mapName, new Coordinates(positionX, positionY), width, height));
                    }
                    catch (FormatException)
                    {
                        throw new InvalidDataException(
                            $"Failed to parse atlasinfo.txt:line {lineNo} - Failed to parse number");
                    }
                }
                else
                {
                    throw new InvalidDataException(
                        $"Failed to parse atlasinfo.txt:line {lineNo} - Failed to parse line");
                }
            }
        }

        var maps = _configuration.GetSection("maps").Get<string[]>() ?? [];
        var townCoords = await Task.WhenAll(maps.Select(GetTownCordsAsync));
        var townCoordsDict = maps.Zip(townCoords).ToDictionary(x => x.First, x => x.Second);
        return atlasValues.Select(val =>
        {
            var (mapName, position, width, height) = val;

            IMap map;
            if (!maps.Contains(mapName))
            {
                map = new RemoteMap(world, mapName, position, width, height);
            }
            else
            {
                townCoordsDict.TryGetValue(mapName, out var coords);

                map = new Map(_monsterManager, _animationManager, _cacheManager, world, _logger, _spawnPointProvider,
                    _attributeProvider, _serviceProvider.GetRequiredService<IDropProvider>(), _itemManager, _server,
                    mapName, position,
                    width,
                    height, coords, _serviceProvider);
            }

            if (position.X + width * Map.MapUnit > maxX) maxX = position.X + width * Map.MapUnit;
            if (position.Y + height * Map.MapUnit > maxY) maxY = position.Y + height * Map.MapUnit;

            return map;
        });
    }

    /// <summary>
    /// Town.txt must contain 1 or 4 lines
    /// </summary>
    private async Task<TownCoordinates?> GetTownCordsAsync(string mapName)
    {
        var fileInfo = _fileProvider.GetFileInfo($"maps/{mapName}/Town.txt");
        var list = new List<Coordinates>();
        if (fileInfo.Exists)
        {
            await using var stream = fileInfo.CreateReadStream();
            using var sr = new StreamReader(stream);
            var i = 0;
            try
            {
                while (!sr.EndOfStream)
                {
                    var line = await sr.ReadLineAsync();

                    var splitted = line.Split(' ');
                    list.Add(new Coordinates(uint.Parse(splitted[0]), uint.Parse(splitted[1])));
                    i++;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to parse line {LineNumber} of town.txt for map {MapName}", i + 1, mapName);
            }
        }

        if (list.Count >= 4)
        {
            return new TownCoordinates {Jinno = list[0], Shinsoo = list[1], Chunjo = list[2], Common = list[3]};
        }
        else if (list.Count == 1)
        {
            return new TownCoordinates {Jinno = list[0], Shinsoo = list[0], Chunjo = list[0], Common = list[0]};
        }
        else
        {
            return null;
        }
    }
}
