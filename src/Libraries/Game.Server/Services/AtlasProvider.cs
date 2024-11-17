using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.World;
using QuantumCore.Caching;
using QuantumCore.Game.World;

namespace QuantumCore.Game.Services;

internal partial class AtlasProvider : IAtlasProvider
{
    private record AtlasValue(string MapName, uint X, uint Y, uint Width, uint Height);

    private static readonly AtlasValue[] DefaultAtlasValues =
    [
        new("metin2_map_a1", 409600, 896000, 4, 5),
        new("metin2_map_b1", 0, 102400, 4, 5),
        new("metin2_map_a1", 921600, 204800, 4, 5),
    ];

    private readonly IConfiguration _configuration;
    private readonly IMonsterManager _monsterManager;
    private readonly IAnimationManager _animationManager;
    private readonly ISpawnPointProvider _spawnPointProvider;
    private readonly ICacheManager _cacheManager;
    private readonly ILogger<AtlasProvider> _logger;
    private readonly IDropProvider _dropProvider;
    private readonly IItemManager _itemManager;
    private readonly IFileProvider _fileProvider;
    private readonly IServerBase _server;

    /// <summary>
    /// Regex for parsing lines in the atlas info
    /// </summary>
    [GeneratedRegex(@"^([a-zA-Z0-9\/_]+)[\s]+([0-9]+)[\s]+([0-9]+)[\s]+([0-9]+)[\s]+([0-9]+)$", RegexOptions.Compiled,
        100)]
    private static partial Regex LineParser();

    public AtlasProvider(IConfiguration configuration, IMonsterManager monsterManager,
        IAnimationManager animationManager, ISpawnPointProvider spawnPointProvider,
        ICacheManager cacheManager, ILogger<AtlasProvider> logger, IDropProvider dropProvider, IItemManager itemManager,
        IFileProvider fileProvider, IServerBase server)
    {
        _configuration = configuration;
        _monsterManager = monsterManager;
        _animationManager = animationManager;
        _spawnPointProvider = spawnPointProvider;
        _cacheManager = cacheManager;
        _logger = logger;
        _dropProvider = dropProvider;
        _itemManager = itemManager;
        _fileProvider = fileProvider;
        _server = server;
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
                        atlasValues.Add(new AtlasValue(mapName, positionX, positionY, width, height));
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
        return atlasValues.Select(val =>
        {
            var (mapName, positionX, positionY, width, height) = val;

            IMap map;
            if (!maps.Contains(mapName))
            {
                map = new RemoteMap(world, mapName, positionX, positionY, width, height);
            }
            else
            {
                map = new Map(_monsterManager, _animationManager, _cacheManager, world, _logger,
                    _spawnPointProvider, _dropProvider, _itemManager, _server, mapName, positionX, positionY,
                    width,
                    height);
            }

            if (positionX + width * Map.MapUnit > maxX) maxX = positionX + width * Map.MapUnit;
            if (positionY + height * Map.MapUnit > maxY) maxY = positionY + height * Map.MapUnit;

            return map;
        });
    }
}
