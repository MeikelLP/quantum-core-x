using System.Security.Cryptography;
using AwesomeAssertions;
using Core.Persistence;
using Game.Caching;
using Game.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuantumCore;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;
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
                    { "job:0:Ht", "4" },
                    { "game:empire:0:x", "0" },
                    { "game:empire:0:y", "0" },
                    { "game:empire:1:x", "10" },
                    { "game:empire:1:y", "15" },
                    { "game:empire:2:x", "20" },
                    { "game:empire:2:y", "25" },
                    { "game:empire:3:x", "30" },
                    { "game:empire:3:y", "35" },
                })
                .Build())
            .AddGameServices()
            .Configure<DatabaseOptions>(HostingOptions.MODE_GAME, opts =>
            {
                opts.ConnectionString = databaseFixture.Container.GetConnectionString();
                opts.Provider = DatabaseProvider.MYSQL;
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
        await _cacheManager.FlushAllAsync();
        await _db.Players.ExecuteDeleteAsync();
        await _scope.DisposeAsync();
    }

    [Theory]
    [InlineData(EEmpire.SHINSOO)]
    [InlineData(EEmpire.CHUNJO)]
    [InlineData(EEmpire.JINNO)]
    public async Task CreateCharacterAsync(EEmpire empire)
    {
        var accountId = Guid.NewGuid();
        await _cachePlayer.SetTempEmpireAsync(accountId, empire);
        var player = await _playerManager.CreateAsync(accountId, "Testificate", 0, 1);

        player.Should().BeEquivalentTo(
            new PlayerData
            {
                AccountId = accountId,
                Name = "Testificate",
                PlayerClass = 0,
                Empire = empire,
                Ht = 4,
                PositionX = (int)_gameOptions.Empire[empire].X,
                PositionY = (int)_gameOptions.Empire[empire].Y
            }, cfg => cfg.Excluding(x => x.Id));
        player.Id.Should().NotBe(0);

        var dbPlayer = await _dbPlayerRepository.GetPlayerAsync(player.Id);
        dbPlayer.Should().BeEquivalentTo(player);

        var playerKey = $"player:{player.Id.ToString()}";
        var accountKey = $"players:{accountId.ToString()}:0";
        (await _cacheManager.Server.KeysAsync("*")).Should().HaveCount(3)
            .And.Contain(playerKey)
            .And.Contain(accountKey)
            .And.Contain($"temp:empire-selection:{accountId}");
        (await _cacheManager.Server.GetAsync<PlayerData>(playerKey)).Should().BeEquivalentTo(player);
    }

    [Fact]
    public async Task IsNameInUseOtherAccountAsync()
    {
        var accountId = Guid.NewGuid();
        await _cachePlayer.SetTempEmpireAsync(accountId, EEmpire.CHUNJO);
        await _playerManager.CreateAsync(accountId, "Testificate", 0, 1);

        var resultCaseSensitive = await _playerManager.IsNameInUseAsync("Testificate");
        var resultCaseInsensitive = await _playerManager.IsNameInUseAsync("testificate");

        resultCaseSensitive.Should().BeTrue();
        resultCaseInsensitive.Should().BeTrue();
    }

    [Fact]
    public async Task GetPlayerByIdAsync()
    {
        var playerId = (uint)RandomNumberGenerator.GetInt32(0, 100);
        var input = new PlayerData { Id = playerId, Name = "1234" };
        await _cacheManager.Server.SetAsync($"player:{playerId}", input);

        var output = await _playerManager.GetPlayerAsync(playerId);

        output.Should().BeEquivalentTo(input);
    }

    [Fact]
    public async Task GetPlayer_OnlyInDb_CreatesCacheAsync()
    {
        var playerId = (uint)RandomNumberGenerator.GetInt32(0, 100);
        await _dbPlayerRepository.CreateAsync(new PlayerData
        {
            Id = playerId, Name = "1234", AccountId = new Guid("AB79A4E3-21E3-4A7A-AB84-C9A94C3DC041")
        });
        var player = await _playerManager.GetPlayerAsync(playerId);

        var keys = await _cacheManager.Server.KeysAsync("*");
        keys.Should().HaveCount(2);
        keys.Should().Contain($"player:{playerId}");
        keys.Should().Contain($"players:{player!.AccountId}:0");
    }

    [Fact]
    public async Task GetPlayerByAccountIdAndSlotAsync()
    {
        var empire = EEmpire.JINNO;
        var accountId = Guid.NewGuid();
        var input1 = new PlayerData
        {
            Name = "1234",
            AccountId = accountId,
            PositionX = (int)_gameOptions.Empire[empire].X,
            PositionY = (int)_gameOptions.Empire[empire].Y,
            Ht = 4,
            Empire = empire
        };
        var input2 = new PlayerData
        {
            Name = "12345",
            AccountId = accountId,
            PositionX = (int)_gameOptions.Empire[empire].X,
            PositionY = (int)_gameOptions.Empire[empire].Y,
            Ht = 4,
            Empire = empire,
            Slot = 1
        };
        await _cachePlayer.SetTempEmpireAsync(accountId, empire);
        await _playerManager.CreateAsync(accountId, input1.Name, 0, 0);
        await _playerManager.CreateAsync(accountId, input2.Name, 0, 0);
        var output1 = await _playerManager.GetPlayerAsync(accountId, 0);
        var output2 = await _playerManager.GetPlayerAsync(accountId, 1);

        output1.Should().BeEquivalentTo(input1, cfg => cfg.Excluding(x => x.Id));
        output2.Should().BeEquivalentTo(input2, cfg => cfg.Excluding(x => x.Id));

        output1!.Id.Should().NotBe(0);
        output2!.Id.Should().NotBe(0);
    }

    [Fact]
    public async Task GetPlayerByAccountIdAndSlot_OnlyInDb_CreatesCacheAsync()
    {
        var playerId = (uint)RandomNumberGenerator.GetInt32(0, 100);
        var accountId = Guid.NewGuid();
        await _dbPlayerRepository.CreateAsync(new PlayerData { AccountId = accountId, Id = playerId, Name = "1234" });
        var player = await _playerManager.GetPlayerAsync(accountId, 0);

        player.Should().NotBeNull();

        var keys = await _cacheManager.Server.KeysAsync("*");
        keys.Should().HaveCount(2).And
            .Contain($"player:{playerId}").And
            .Contain($"players:{accountId}:0");
    }

    [Fact]
    public async Task GetPlayerById_NotFoundAsync()
    {
        var output = await _playerManager.GetPlayerAsync((uint)RandomNumberGenerator.GetInt32(0, 100));

        output.Should().BeNull();
    }

    [Fact]
    public async Task GetPlayerById_WithMultiplePlayers_CachesUnderCorrectSlotAsync()
    {
        await _cacheManager.FlushAllAsync();

        var accountId = Guid.NewGuid();

        const uint FIRST_ID = 100u, SECOND_ID = 200u, THIRD_ID = 300u;
        await _dbPlayerRepository.CreateAsync(new PlayerData
        {
            Id = FIRST_ID, AccountId = accountId, Name = "PlayerA"
        });
        await _dbPlayerRepository.CreateAsync(
            new PlayerData { Id = SECOND_ID, AccountId = accountId, Name = "PlayerB" });
        await _dbPlayerRepository.CreateAsync(new PlayerData
        {
            Id = THIRD_ID, AccountId = accountId, Name = "PlayerC"
        });

        var fetched = await _playerManager.GetPlayerAsync(SECOND_ID);

        fetched.Should().NotBeNull();
        fetched.Id.Should().Be(SECOND_ID);

        var keys = await _cacheManager.Server.KeysAsync("*");
        keys.Should().HaveCount(2)
            .And.Contain($"player:{SECOND_ID}")
            .And.Contain($"players:{accountId}:1");
        keys.Should().NotContain($"players:{accountId}:0");
        keys.Should().NotContain($"players:{accountId}:2");
    }

    [Fact]
    public async Task GetPlayerByAccountIdAndSlot_NotFoundAsync()
    {
        var output = await _playerManager.GetPlayerAsync(Guid.NewGuid(), 0);

        output.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCharacterAsync()
    {
        var accountId = Guid.NewGuid();
        await _cachePlayer.SetTempEmpireAsync(accountId, EEmpire.CHUNJO);
        var player = await _playerManager.CreateAsync(accountId, "Testificate", 0, 1);
        await _playerManager.DeletePlayerAsync(player);

        (await _cacheManager.Server.KeysAsync("*")).Should().BeEquivalentTo([$"temp:empire-selection:{accountId}"]);
        (await _dbPlayerRepository.GetPlayersAsync(accountId)).Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task GetPlayers_ReturnsOrderedAndCachesWithSlotsAsync(int charactersCount)
    {
        await _cacheManager.FlushAllAsync();
        await _db.Players.ExecuteDeleteAsync();

        var accountId = Guid.NewGuid();
        var basePlayerId = 1000u;

        for (uint i = 0; i < charactersCount; i++)
        {
            var id = basePlayerId + i;
            await _dbPlayerRepository.CreateAsync(
                new PlayerData { Id = id, AccountId = accountId, Name = $"Player{i}" });
        }

        var players = await _playerManager.GetPlayersAsync(accountId);

        players.Should().HaveCount(charactersCount);
        for (var i = 0; i < charactersCount; i++)
        {
            players[i].Slot.Should().Be((byte)i);

            var expectedId = basePlayerId + (uint)i;
            players[i].Id.Should().Be(expectedId);
        }

        var keys = await _cacheManager.Server.KeysAsync("*");
        keys.Should().HaveCount(charactersCount * 2);
        for (var i = 0; i < charactersCount; i++)
        {
            var expectedId = basePlayerId + i;
            keys.Should().Contain($"player:{expectedId}");
            keys.Should().Contain($"players:{accountId}:{i}");
        }
    }
}