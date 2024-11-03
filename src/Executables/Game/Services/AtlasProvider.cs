using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuantumCore.API;
using QuantumCore.API.Game.World;
using QuantumCore.Caching;
using QuantumCore.Game.World;

namespace QuantumCore.Game.Services;

internal partial class AtlasProvider : IAtlasProvider
{
    private readonly IConfiguration _configuration;
    private readonly IMonsterManager _monsterManager;
    private readonly IAnimationManager _animationManager;
    private readonly ISpawnPointProvider _spawnPointProvider;
    private readonly IOptions<HostingOptions> _options;
    private readonly ICacheManager _cacheManager;
    private readonly ILogger<AtlasProvider> _logger;
    private readonly IDropProvider _dropProvider;
    private readonly IItemManager _itemManager;
    private readonly IFileProvider _fileProvider;

    /// <summary>
    /// Regex for parsing lines in the atlas info
    /// </summary>
    [GeneratedRegex(@"^([a-zA-Z0-9\/_]+)[\s]+([0-9]+)[\s]+([0-9]+)[\s]+([0-9]+)[\s]+([0-9]+)$", RegexOptions.Compiled,
        100)]
    private static partial Regex LineParser();

    public AtlasProvider(IConfiguration configuration, IMonsterManager monsterManager,
        IAnimationManager animationManager, ISpawnPointProvider spawnPointProvider, IOptions<HostingOptions> options,
        ICacheManager cacheManager, ILogger<AtlasProvider> logger, IDropProvider dropProvider, IItemManager itemManager,
        IFileProvider fileProvider)
    {
        _configuration = configuration;
        _monsterManager = monsterManager;
        _animationManager = animationManager;
        _spawnPointProvider = spawnPointProvider;
        _options = options;
        _cacheManager = cacheManager;
        _logger = logger;
        _dropProvider = dropProvider;
        _itemManager = itemManager;
        _fileProvider = fileProvider;
    }

    public async Task<IEnumerable<IMap>> GetAsync(IWorld world)
    {
        var maxX = 0u;
        var maxY = 0u;

        // Load atlasinfo.txt and initialize all maps the game core hosts
        var fileInfo = _fileProvider.GetFileInfo("atlasinfo.txt");
        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException($"Unable to find file {fileInfo.PhysicalPath}");
        }

        var maps = _configuration.GetSection("maps").Get<string[]>() ?? [];
        var returnList = new List<IMap>();
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

                    IMap map;
                    if (!maps.Contains(mapName))
                    {
                        map = new RemoteMap(world, mapName, positionX, positionY, width, height);
                    }
                    else
                    {
                        map = new Map(_monsterManager, _animationManager, _cacheManager, world, _options, _logger,
                            _spawnPointProvider, _dropProvider, _itemManager, mapName, positionX, positionY, width,
                            height);
                    }


                    returnList.Add(map);

                    if (positionX + width * Map.MapUnit > maxX) maxX = positionX + width * Map.MapUnit;
                    if (positionY + height * Map.MapUnit > maxY) maxY = positionY + height * Map.MapUnit;
                }
                catch (FormatException)
                {
                    throw new InvalidDataException(
                        $"Failed to parse atlasinfo.txt:line {lineNo} - Failed to parse number");
                }
            }
            else
            {
                throw new InvalidDataException($"Failed to parse atlasinfo.txt:line {lineNo} - Failed to parse line");
            }
        }

        return returnList;
    }
}
