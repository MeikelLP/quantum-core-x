using FluentAssertions;
using Game.Caching;
using Game.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuantumCore;
using QuantumCore.API.Core.Models;
using QuantumCore.Caching;
using QuantumCore.Game;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Persistence;
using Serilog;
using Xunit.Abstractions;

namespace Game.Tests;

public class PlayerManagerTests : IClassFixture<RedisFixture>, IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly IPlayerManager _playerManager;
    private readonly IDbPlayerRepository _dbPlayerRepository;
    private readonly ICacheManager _cacheManager;
    private readonly ICachePlayerRepository _cachePlayer;
    private readonly AsyncServiceScope _scope;
    private readonly MySqlGameDbContext _db;
    private readonly GameOptions _gameOptions;

    public PlayerManagerTests(ITestOutputHelper outputHelper, RedisFixture redisFixture,
        DatabaseFixture databaseFixture)
    {
        var services = new ServiceCollection()
            .AddLogging(cfg => cfg
                .ClearProviders()
                .AddSerilog(new LoggerConfiguration()
                    .WriteTo.TestOutput(outputHelper)
                    .CreateLogger()))
            .AddSingleton<IConfiguration>(_ => new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"job:0:Ht", "4"},
                    {"game:empire:0:x", "0"},
                    {"game:empire:0:y", "0"},
                    {"game:empire:1:x", "10"},
                    {"game:empire:1:y", "15"},
                    {"game:empire:2:x", "20"},
                    {"game:empire:2:y", "25"},
                    {"game:empire:3:x", "30"},
                    {"game:empire:3:y", "35"},
                })
                .Build())
            .AddGameServices()
            .Configure<DatabaseOptions>(HostingOptions.ModeGame, opts =>
            {
                opts.ConnectionString = databaseFixture.Container.GetConnectionString();
                opts.Provider = DatabaseProvider.Mysql;
            })
            .Configure<CacheOptions>(opts => { opts.Port = redisFixture.Container.GetMappedPublicPort(6379); })
            .BuildServiceProvider();
        _playerManager = services.GetRequiredService<IPlayerManager>();
        _dbPlayerRepository = services.GetRequiredService<IDbPlayerRepository>();
        _cacheManager = services.GetRequiredService<ICacheManager>();
        _cachePlayer = services.GetRequiredService<ICachePlayerRepository>();
        _scope = services.CreateAsyncScope();
        _db = _scope.ServiceProvider.GetRequiredService<MySqlGameDbContext>();
        _gameOptions = services.GetRequiredService<IOptions<GameOptions>>().Value;
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _cacheManager.FlushAll();
        await _db.Players.ExecuteDeleteAsync();
        await _scope.DisposeAsync();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task CreateCharacter(byte empire)
    {
        var accountId = Guid.NewGuid();
        await _cachePlayer.SetTempEmpireAsync(accountId, empire);
        var player = await _playerManager.CreateAsync(accountId, "Testificate", 0, 1);

        player.Should().BeEquivalentTo(new PlayerData
        {
            AccountId = accountId,
            Name = "Testificate",
            PlayerClass = 0,
            Empire = empire,
            Ht = 4,
            PositionX = _gameOptions.Empire[empire].X,
            PositionY = _gameOptions.Empire[empire].Y
        }, cfg => cfg.Excluding(x => x.Id));
        player.Id.Should().NotBe(0);

        var dbPlayer = await _dbPlayerRepository.GetPlayerAsync(player.Id);
        dbPlayer.Should().BeEquivalentTo(player);

        var playerKey = $"player:{player.Id.ToString()}";
        var accountKey = $"players:{accountId.ToString()}:0";
        (await _cacheManager.Server.Keys("*")).Should().HaveCount(3)
            .And.Contain(playerKey)
            .And.Contain(accountKey)
            .And.Contain($"temp:empire-selection:{accountId}");
        (await _cacheManager.Server.Get<PlayerData>(playerKey)).Should().BeEquivalentTo(player);
    }

    [Fact]
    public async Task IsNameInUseOtherAccount()
    {
        var accountId = Guid.NewGuid();
        await _cachePlayer.SetTempEmpireAsync(accountId, 2);
        await _playerManager.CreateAsync(accountId, "Testificate", 0, 1);

        var resultCaseSensitive = await _playerManager.IsNameInUseAsync("Testificate");
        var resultCaseInsensitive = await _playerManager.IsNameInUseAsync("testificate");

        resultCaseSensitive.Should().BeTrue();
        resultCaseInsensitive.Should().BeTrue();
    }

    [Fact]
    public async Task GetPlayerById()
    {
        var playerId = (uint) Random.Shared.Next();
        var input = new PlayerData
        {
            Id = playerId,
            Name = "1234"
        };
        await _cacheManager.Server.Set($"player:{playerId}", input);

        var output = await _playerManager.GetPlayer(playerId);

        output.Should().BeEquivalentTo(input);
    }

    [Fact]
    public async Task GetPlayer_OnlyInDb_CreatesCache()
    {
        var playerId = (uint) Random.Shared.Next();
        await _dbPlayerRepository.CreateAsync(new PlayerData
        {
            Id = playerId,
            Name = "1234",
            AccountId = new Guid("AB79A4E3-21E3-4A7A-AB84-C9A94C3DC041")
        });
        var player = await _playerManager.GetPlayer(playerId);

        var keys = await _cacheManager.Server.Keys("*");
        keys.Should().HaveCount(2);
        keys.Should().Contain($"player:{playerId}");
        keys.Should().Contain($"players:{player!.AccountId}:0");
    }

    [Fact]
    public async Task GetPlayerByAccountIdAndSlot()
    {
        byte empire = 3;
        var accountId = Guid.NewGuid();
        var input1 = new PlayerData
        {
            Name = "1234",
            AccountId = accountId,
            PositionX = _gameOptions.Empire[empire].X,
            PositionY = _gameOptions.Empire[empire].Y,
            Ht = 4,
            Empire = empire
        };
        var input2 = new PlayerData
        {
            Name = "12345",
            AccountId = accountId,
            PositionX = _gameOptions.Empire[empire].X,
            PositionY = _gameOptions.Empire[empire].Y,
            Ht = 4,
            Empire = empire,
            Slot = 1
        };
        await _cachePlayer.SetTempEmpireAsync(accountId, empire);
        await _playerManager.CreateAsync(accountId, input1.Name, 0, 0);
        await _playerManager.CreateAsync(accountId, input2.Name, 0, 0);
        var output1 = await _playerManager.GetPlayer(accountId, 0);
        var output2 = await _playerManager.GetPlayer(accountId, 1);

        output1.Should().BeEquivalentTo(input1, cfg => cfg.Excluding(x => x.Id));
        output2.Should().BeEquivalentTo(input2, cfg => cfg.Excluding(x => x.Id));

        output1!.Id.Should().NotBe(0);
        output2!.Id.Should().NotBe(0);
    }

    [Fact]
    public async Task GetPlayerByAccountIdAndSlot_OnlyInDb_CreatesCache()
    {
        var playerId = (uint) Random.Shared.Next();
        var accountId = Guid.NewGuid();
        await _dbPlayerRepository.CreateAsync(new PlayerData
        {
            AccountId = accountId,
            Id = playerId,
            Name = "1234"
        });
        var player = await _playerManager.GetPlayer(accountId, 0);

        player.Should().NotBeNull();

        var keys = await _cacheManager.Server.Keys("*");
        keys.Should().HaveCount(2).And
            .Contain($"player:{playerId}").And
            .Contain($"players:{accountId}:0");
    }

    [Fact]
    public async Task GetPlayerById_NotFound()
    {
        var output = await _playerManager.GetPlayer((uint) Random.Shared.Next());

        output.Should().BeNull();
    }

    [Fact]
    public async Task GetPlayerByAccountIdAndSlot_NotFound()
    {
        var output = await _playerManager.GetPlayer(Guid.NewGuid(), 0);

        output.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCharacter()
    {
        var accountId = Guid.NewGuid();
        await _cachePlayer.SetTempEmpireAsync(accountId, 2);
        var player = await _playerManager.CreateAsync(accountId, "Testificate", 0, 1);
        await _playerManager.DeletePlayerAsync(player);

        (await _cacheManager.Server.Keys("*")).Should().BeEquivalentTo([$"temp:empire-selection:{accountId}"]);
        (await _dbPlayerRepository.GetPlayersAsync(accountId)).Should().BeEmpty();
    }
}