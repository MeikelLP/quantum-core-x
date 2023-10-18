using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuantumCore.API;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Cache;
using QuantumCore.Game.World;

namespace QuantumCore.Game.Services;

internal class AtlasProvider : IAtlasProvider
{
    private readonly IConfiguration _configuration;
    private readonly IMonsterManager _monsterManager;
    private readonly IAnimationManager _animationManager;
    private readonly ISpawnPointProvider _spawnPointProvider;
    private readonly IOptions<HostingOptions> _options;
    private readonly ICacheManager _cacheManager;
    private readonly ILogger<AtlasProvider> _logger;

    public AtlasProvider(IConfiguration configuration, IMonsterManager monsterManager,
        IAnimationManager animationManager, ISpawnPointProvider spawnPointProvider, IOptions<HostingOptions> options,
        ICacheManager cacheManager, ILogger<AtlasProvider> logger)
    {
        _configuration = configuration;
        _monsterManager = monsterManager;
        _animationManager = animationManager;
        _spawnPointProvider = spawnPointProvider;
        _options = options;
        _cacheManager = cacheManager;
        _logger = logger;
    }

    public async Task<IEnumerable<IMap>> GetAsync(IWorld world)
    {
        // Regex for parsing lines in the atlas info
        var regex = new Regex(@"^([a-zA-Z0-9\/_]+)[\s]+([0-9]+)[\s]+([0-9]+)[\s]+([0-9]+)[\s]+([0-9]+)$");

        var maxX = 0u;
        var maxY = 0u;

        // Load atlasinfo.txt and initialize all maps the game core hosts
        if (!File.Exists("data/atlasinfo.txt"))
        {
            throw new FileNotFoundException("Unable to find file data/atlasinfo.txt");
        }

        var maps = _configuration.GetSection("maps").Get<string[]>() ?? Array.Empty<string>();
        var returnList = new List<IMap>();
        using var reader = new StreamReader("data/atlasinfo.txt");
        string line;
        var lineNo = 0;
        while ((line = (await reader.ReadLineAsync())!) != null)
        {
            lineNo++;
            if (string.IsNullOrWhiteSpace(line)) continue; // skip empty lines

            var match = regex.Match(line);
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
                        map = new RemoteMap(mapName, positionX, positionY, width, height);
                    }
                    else
                    {
                        map = new Map(_monsterManager, _animationManager, _cacheManager, world, _options, _logger,
                            _spawnPointProvider, mapName, positionX, positionY, width, height);
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
