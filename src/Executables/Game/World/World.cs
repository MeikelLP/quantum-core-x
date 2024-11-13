using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.API.PluginTypes;
using QuantumCore.Caching;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Services;

namespace QuantumCore.Game.World
{
    public class World : IWorld
    {
        private readonly ILogger<World> _logger;
        private readonly PluginExecutor _pluginExecutor;
        private uint _vid;
        private readonly Grid<IMap> _world = new(0, 0);
        private readonly Dictionary<string, IMap> _maps = new();
        private readonly Dictionary<string, IPlayerEntity> _players = new();
        private readonly Dictionary<uint, SpawnGroup> _groups = new();
        private readonly Dictionary<uint, SpawnGroupCollection> _groupCollections = new();

        private readonly Dictionary<int, Shop> _staticShops = new();

        private IRedisSubscriber? _mapSubscriber;
        private readonly IItemManager _itemManager;
        private readonly ICacheManager _cacheManager;
        private readonly IConfiguration _configuration;
        private readonly ISpawnGroupProvider _spawnGroupProvider;
        private readonly IAtlasProvider _atlasProvider;

        public World(ILogger<World> logger, PluginExecutor pluginExecutor, IItemManager itemManager,
            ICacheManager cacheManager, IConfiguration configuration, ISpawnGroupProvider spawnGroupProvider,
            IAtlasProvider atlasProvider)
        {
            _logger = logger;
            _pluginExecutor = pluginExecutor;
            _itemManager = itemManager;
            _cacheManager = cacheManager;
            _configuration = configuration;
            _spawnGroupProvider = spawnGroupProvider;
            _atlasProvider = atlasProvider;
            _vid = 0;
        }

        public async Task Load()
        {
            LoadShops();
            var groups = await _spawnGroupProvider.GetSpawnGroupsAsync();
            foreach (var g in groups)
            {
                _groups[g.Id] = g;
            }

            var spawnGroups = await _spawnGroupProvider.GetSpawnGroupCollectionsAsync();
            foreach (var g in spawnGroups)
            {
                _groupCollections[g.Id] = g;
            }

            var maps = await _atlasProvider.GetAsync(this);
            foreach (var map in maps)
            {
                _maps[map.Name] = map;
            }


            // Initialize world grid and place maps on it
            var maxX = _maps.Max(x => x.Value.PositionX + x.Value.Width * Map.MapUnit);
            var maxY = _maps.Max(x => x.Value.PositionY + x.Value.Height * Map.MapUnit);
            _world.Resize(maxX / Map.MapUnit, maxY / Map.MapUnit);
            foreach (var map in _maps.Values)
            {
                for (var x = map.UnitX; x < map.UnitX + map.Width; x++)
                {
                    for (var y = map.UnitY; y < map.UnitY + map.Height; y++)
                    {
                        _world.Set(x, y, map);
                    }
                }
            }

            await LoadRemoteMaps();

            // Initialize maps, spawn monsters etc
            foreach (var map in _maps.Values)
            {
                if (map is Map m)
                {
                    await m.Initialize();
                }
            }
        }

        private void LoadShops()
        {
            var shops = _configuration.GetSection("shops").Get<ShopDefinition[]>();
            if (shops is null) return;
            foreach (var shopDef in shops)
            {
                var shop = new Shop(_itemManager, _logger);

                _staticShops[shopDef.Id] = shop;

                if (shopDef.Npc.HasValue)
                {
                    GameEventManager.RegisterNpcClickEvent(shop.Name, shopDef.Npc.Value, player =>
                    {
                        shop.Open(player);
                        return Task.CompletedTask;
                    });
                }
            }
        }

        private async Task LoadRemoteMaps()
        {
            var keys = await _cacheManager.Keys("maps:*");

            foreach (var key in keys)
            {
                var mapName = key[5..];
                var map = _maps[mapName];
                if (map is not RemoteMap remoteMap)
                {
                    continue;
                }

                var address = await _cacheManager.Get<string>(key);
                var parts = address.Split(":");
                Debug.Assert(parts.Length == 2);

                remoteMap.Host = IPAddress.Parse(parts[0]);
                remoteMap.Port = ushort.Parse(parts[1]);

                _logger.LogDebug("Map {Name} is available at {Host}:{Port}", remoteMap.Name, remoteMap.Host,
                    remoteMap.Port);
            }

            _mapSubscriber = _cacheManager.Subscribe();
            _mapSubscriber.Register<string>("maps", mapDetails =>
            {
                var data = mapDetails.Split(" ");
                Debug.Assert(data.Length == 2);

                var mapName = data[0];
                var parts = data[1].Split(":");
                Debug.Assert(parts.Length == 2);

                var map = _maps[mapName];
                if (map is not RemoteMap remoteMap)
                {
                    return;
                }

                remoteMap.Host = IPAddress.Parse(parts[0]);
                remoteMap.Port = ushort.Parse(parts[1]);

                _logger.LogDebug("Map {Name} is now available at {Host}:{Port}", remoteMap.Name, remoteMap.Host,
                    remoteMap.Port);
            });

            _mapSubscriber.Listen();
        }

        public void Update(double elapsedTime)
        {
            // HookManager.Instance.CallHook<IHookWorldUpdate>(elapsedTime);

            foreach (var map in _maps.Values)
            {
                map.Update(elapsedTime);
            }
        }

        public IMap? GetMapAt(uint x, uint y)
        {
            var gridX = x / Map.MapUnit;
            var gridY = y / Map.MapUnit;

            return _world.Get(gridX, gridY);
        }

        public IMap? GetMapByName(string name)
        {
            return _maps.TryGetValue(name, out var map) ? map : null;
        }

        public List<IMap> FindMapsByName(string needle)
        {
            var list = new List<IMap>();
            foreach (var (name, map) in _maps)
            {
                if (name == needle)
                {
                    list.Clear();
                    list.Add(map);
                    return list;
                }

                if (name.Contains(needle, StringComparison.InvariantCultureIgnoreCase))
                {
                    list.Add(map);
                }
            }

            return list;
        }

        public CoreHost GetMapHost(int x, int y)
        {
            var map = GetMapAt((uint) x, (uint) y);
            if (map == null)
            {
                _logger.LogWarning("No available host for map at {X}|{Y}", x, y);
                return new CoreHost {Ip = IPAddress.None, Port = 0};
            }

            if (map is RemoteMap remoteMap)
            {
                if (remoteMap.Host is null)
                {
                    _logger.LogCritical("Remote maps IP is null. This should never happen. Name: {MapName}",
                        remoteMap.Name);
                    throw new InvalidOperationException("Cannot handle this situation. See logs.");
                }

                return new CoreHost
                {
                    Ip = remoteMap.Host,
                    Port = remoteMap.Port
                };
            }

            return new CoreHost
            {
                Ip = IpUtils.PublicIP!, // should be set by now
                Port = (ushort) GameServer.Instance.Port
            };
        }

        public SpawnGroup? GetGroup(uint id)
        {
            if (!_groups.ContainsKey(id))
            {
                return null;
            }

            return _groups[id];
        }

        public SpawnGroupCollection? GetGroupCollection(uint id)
        {
            if (!_groupCollections.ContainsKey(id))
            {
                return null;
            }

            return _groupCollections[id];
        }

        public void SpawnEntity(IEntity e)
        {
            var map = GetMapAt((uint) e.PositionX, (uint) e.PositionY);
            if (map == null)
            {
                _logger.LogWarning("Could not spawn entity at ({X};{Y}) No Map found for this coordinate", e.PositionX,
                    e.PositionY);
                return;
            }

            if (map is RemoteMap)
            {
                _logger.LogWarning("Cannot spawn entity on RemoteMap. This is not implemented yet");
                return;
            }

            if (e is IPlayerEntity player)
            {
                AddPlayer(player);
                _logger.LogInformation("Player {PlayerName} ({PlayerId}) joined the map {MapName}", player.Name,
                    player.Vid, map.Name);
            }

            _pluginExecutor.ExecutePlugins<IGameEntityLifetimeListener>(_logger, x => x.OnPreCreatedAsync()).Wait();
            map.SpawnEntity(e);

            _pluginExecutor.ExecutePlugins<IGameEntityLifetimeListener>(_logger, x => x.OnPostCreatedAsync()).Wait();
        }

        public void DespawnEntity(IEntity entity)
        {
            if (entity is IPlayerEntity player)
            {
                RemovePlayer(player);
            }

            _pluginExecutor.ExecutePlugins<IGameEntityLifetimeListener>(_logger, x => x.OnPreDeletedAsync()).Wait();
            entity.Map?.DespawnEntity(entity);
            _pluginExecutor.ExecutePlugins<IGameEntityLifetimeListener>(_logger, x => x.OnPostDeletedAsync()).Wait();
        }

        public async Task DespawnPlayerAsync(IPlayerEntity player)
        {
            RemovePlayer(player);
            await player.OnDespawnAsync();

            _pluginExecutor.ExecutePlugins<IGameEntityLifetimeListener>(_logger, x => x.OnPreDeletedAsync()).Wait();
            player.Map?.DespawnEntity(player);
            _pluginExecutor.ExecutePlugins<IGameEntityLifetimeListener>(_logger, x => x.OnPostDeletedAsync()).Wait();
        }

        public uint GenerateVid()
        {
            return ++_vid;
        }

        private void AddPlayer(IPlayerEntity e)
        {
            if (_players.ContainsKey(e.Name))
                _players[e.Name] = e;
            else
                _players.Add(e.Name, e);
        }

        public void RemovePlayer(IPlayerEntity e)
        {
            _players.Remove(e.Name);
        }

        public IPlayerEntity? GetPlayer(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
            {
                return null;
            }

            return _players.TryGetValue(playerName, out var player) ? player : null;
        }

        public IList<IPlayerEntity> GetPlayers()
        {
            return _players.Values.ToList();
        }

        public IPlayerEntity? GetPlayerById(uint playerId)
        {
            return _players.Values.FirstOrDefault(x => x.Vid == playerId);
        }
    }
}
