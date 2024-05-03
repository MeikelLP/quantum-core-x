using System.Data;
using Dapper;
using Data.Tests.Fixtures;
using FluentAssertions;
using Game.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using QuantumCore;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.Auth.Persistence;
using QuantumCore.Caching;
using QuantumCore.Game;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Persistence;
using Serilog;
using Testcontainers.MySql;
using Xunit;
using Xunit.Abstractions;

namespace Data.Tests;

public class PlayerManagerTests : IClassFixture<RedisFixture>, IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly IPlayerManager _playerManager;
    private readonly IAccountManager _accountManager;
    private readonly IDbPlayerRepository _dbPlayerRepository;
    private readonly ICacheManager _cacheManager;

    private readonly List<AccountData> _accountRepositoryList = new();
    private readonly IDbConnection _db;
    private readonly ICachePlayerRepository _cachePlayer;

    public PlayerManagerTests(ITestOutputHelper outputHelper, RedisFixture redisFixture, DatabaseFixture databaseFixture)
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
                    { "job:0:Ht", "4" }
                })
                .Build())
            .AddGameServices()
            .Replace(new ServiceDescriptor(typeof(IAccountRepository), _ =>
            {
                var mock = new Mock<IAccountRepository>();
                mock.Setup(x => x.FindByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Guid id) =>
                    _accountRepositoryList.FirstOrDefault(x => x.Id == id));
                mock.Setup(x => x.CreateAsync(It.IsAny<AccountData>())).Callback<AccountData>(account => {_accountRepositoryList.Add(account);});
                return mock.Object;
            }, ServiceLifetime.Singleton))
            .Configure<DatabaseOptions>(opts =>
            {
                opts.Port = databaseFixture.Container.GetMappedPublicPort(MySqlBuilder.MySqlPort);
                opts.Password = DatabaseFixture.Password;
                opts.User = DatabaseFixture.UserName;
                opts.Database = DatabaseFixture.Database;
            })
            .Configure<CacheOptions>(opts =>
            {
                opts.Port = redisFixture.Container.GetMappedPublicPort(6379);
            })
            .BuildServiceProvider();
        _playerManager = services.GetRequiredService<IPlayerManager>();
        _accountManager = services.GetRequiredService<IAccountManager>();
        _dbPlayerRepository = services.GetRequiredService<IDbPlayerRepository>();
        _cacheManager = services.GetRequiredService<ICacheManager>();
        _cachePlayer = services.GetRequiredService<ICachePlayerRepository>();
        _db = services.GetRequiredService<IDbConnection>();
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        var keys = await _cacheManager.Keys("*");
        foreach (var key in keys)
        {
            await _cacheManager.Del(key);
        }

        await _db.ExecuteAsync("""
        DELETE FROM account.accounts;
        DELETE FROM game.players;
        """);
    }

    [Fact]
    public async Task CreateCharacter()
    {
        var account = await _accountManager.CreateAsync("testificate", "testificate", "some@gmail.com", "1234567");
        await _cachePlayer.SetTempEmpireAsync(account.Id, 2);
        var player = await _playerManager.CreateAsync(account.Id, "Testificate", 0, 1);

        player.Should().BeEquivalentTo(new PlayerData
        {
            AccountId = account.Id,
            Name = "Testificate",
            PlayerClass = 0,
            Empire = 2,
            Ht = 4,
            PositionX = 958870,
            PositionY = 272788
        }, cfg => cfg.Excluding(x => x.Id));
        player.Id.Should().NotBeEmpty();

        var dbPlayer = await _dbPlayerRepository.GetPlayerAsync(player.Id);
        dbPlayer.Should().BeEquivalentTo(player);

        var playerKey = $"player:{player.Id.ToString()}";
        var accountKey = $"players:{account.Id.ToString()}:0";
        (await _cacheManager.Keys("*")).Should().HaveCount(3)
            .And.Contain(playerKey)
            .And.Contain(accountKey)
            .And.Contain($"temp:empire-selection:{account.Id}");
        (await _cacheManager.Get<PlayerData>(playerKey)).Should().BeEquivalentTo(player);
    }

    [Fact]
    public async Task IsNameInUseOtherAccount()
    {
        var account = await _accountManager.CreateAsync("testificate", "testificate", "some@gmail.com", "1234567");
        await _cachePlayer.SetTempEmpireAsync(account.Id, 2);
        await _playerManager.CreateAsync(account.Id, "Testificate", 0, 1);

        var resultCaseSensitive = await _playerManager.IsNameInUseAsync("Testificate");
        var resultCaseInsensitive = await _playerManager.IsNameInUseAsync("testificate");

        resultCaseSensitive.Should().BeTrue();
        resultCaseInsensitive.Should().BeTrue();
    }

    [Fact]
    public async Task GetPlayerById()
    {
        var playerId = Guid.NewGuid();
        var input = new PlayerData
        {
            Id = playerId,
            Name = "1234"
        };
        await _cacheManager.Set($"player:{playerId}", input);

        var output = await _playerManager.GetPlayer(playerId);

        output.Should().BeEquivalentTo(input);
    }

    [Fact]
    public async Task GetPlayer_OnlyInDb_CreatesCache()
    {
        var playerId = Guid.NewGuid();
        await _dbPlayerRepository.CreateAsync(new PlayerData
        {
            Id = playerId,
            Name = "1234",
            AccountId = new Guid("AB79A4E3-21E3-4A7A-AB84-C9A94C3DC041")
        });
        var player = await _playerManager.GetPlayer(playerId);

        var keys = await _cacheManager.Keys("*");
        keys.Should().HaveCount(2);
        keys.Should().Contain($"player:{playerId}");
        keys.Should().Contain($"players:{player!.AccountId}:0");
    }

    [Fact]
    public async Task GetPlayerByAccountIdAndSlot()
    {
        var accountId = Guid.NewGuid();
        var input1 = new PlayerData
        {
            Name = "1234",
            AccountId = accountId,
            PositionX = 958870,
            PositionY = 272788,
            Ht = 4,
            Empire = 2
        };
        var input2 = new PlayerData
        {
            Name = "12345",
            AccountId = accountId,
            PositionX = 958870,
            PositionY = 272788,
            Ht = 4,
            Empire = 2,
            Slot = 1
        };
        await _cachePlayer.SetTempEmpireAsync(accountId, 2);
        await _playerManager.CreateAsync(accountId, input1.Name, 0, 0);
        await _playerManager.CreateAsync(accountId, input2.Name, 0, 0);
        var output1 = await _playerManager.GetPlayer(accountId, 0);
        var output2 = await _playerManager.GetPlayer(accountId, 1);

        output1.Should().BeEquivalentTo(input1, cfg => cfg.Excluding(x => x.Id));
        output2.Should().BeEquivalentTo(input2, cfg => cfg.Excluding(x => x.Id));

        output1!.Id.Should().NotBeEmpty();
        output2!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetPlayerByAccountIdAndSlot_OnlyInDb_CreatesCache()
    {
        var playerId = Guid.NewGuid();
        var acc = await _accountManager.CreateAsync("abc", "def", "some@mail.com", "1234567");
        await _dbPlayerRepository.CreateAsync(new PlayerData
        {
            AccountId = acc.Id,
            Id = playerId,
            Name = "1234"
        });
        var player = await _playerManager.GetPlayer(acc.Id, 0);

        player.Should().NotBeNull();

        var keys = await _cacheManager.Keys("*");
        keys.Should().HaveCount(2).And
            .Contain($"player:{playerId}").And
            .Contain($"players:{acc.Id}:0");
    }

    [Fact]
    public async Task GetPlayerById_NotFound()
    {
        var output = await _playerManager.GetPlayer(Guid.NewGuid());

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
        var account = await _accountManager.CreateAsync("testificate", "testificate", "some@gmail.com", "1234567");
        await _cachePlayer.SetTempEmpireAsync(account.Id, 2);
        var player = await _playerManager.CreateAsync(account.Id, "Testificate", 0, 1);
        await _playerManager.DeletePlayerAsync(player);

        (await _cacheManager.Keys("*")).Should().BeEquivalentTo([$"temp:empire-selection:{account.Id}"]);
        (await _dbPlayerRepository.GetPlayersAsync(account.Id)).Should().BeEmpty();
    }
}
