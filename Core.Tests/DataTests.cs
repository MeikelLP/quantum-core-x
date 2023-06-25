using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.Auth.Persistence;
using QuantumCore.Caching;
using QuantumCore.Game;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Persistence;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Core.Tests;

public class DataTests
{
    private class MockedRedisListCache<T> : IRedisListWrapper<T>
    {
        private readonly List<T> _list = new();
        
        public ValueTask<T> Index(int slot)
        {
            return new ValueTask<T>(_list[slot]);
        }

        public ValueTask<T[]> Range(int start, int stop)
        {
            return new ValueTask<T[]>(_list.GetRange(start, stop).ToArray());
        }

        public ValueTask<long> Push(params T[] arr)
        {
             _list.AddRange(arr);
             return new ValueTask<long>(arr.Length);
        }

        public ValueTask<long> Rem(int count, T obj)
        {
            _list.Remove(obj);
            return new ValueTask<long>(1);
        }
    }
    private class MockedCacheManager : ICacheManager
    {
        internal readonly Dictionary<string, object> _cache = new();
        public IRedisListWrapper<T> CreateList<T>(string name)
        {
            return new MockedRedisListCache<T>();
        }

        public ValueTask<long> Del(string key)
        {
            _cache.Remove(key);
            return new ValueTask<long>(1);
        }

        public ValueTask<string> Set(string key, object item)
        {
            _cache.Add(key, item);
            return new ValueTask<string>(key);
        }

        public ValueTask<T> Get<T>(string key)
        {
            return new ValueTask<T>((T)_cache[key]);
        }

        public ValueTask<long> Exists(string key)
        {
            return new ValueTask<long>(_cache.ContainsKey(key) ? 1 : 0);
        }

        public ValueTask<long> Expire(string key, int seconds)
        {
            return new ValueTask<long>(0);
        }

        public ValueTask<bool> Ping()
        {
            return new ValueTask<bool>(true);
        }

        public ValueTask<long> Publish(string key, object obj)
        {
            _cache.Add(key, obj);
            return new ValueTask<long>(1);
        }

        public IRedisSubscriber Subscribe()
        {
            return new Mock<IRedisSubscriber>().Object;
        }

        public ValueTask<string[]> Keys(string key)
        {
            return new ValueTask<string[]>(new[] { key });
        }

        public ValueTask<long> Persist(string key)
        {
            return new ValueTask<long>(1);
        }
    }
    
    private readonly IPlayerManager _playerManager;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAccountManager _accountManager;
    private readonly IDbPlayerRepository _dbPlayerRepository;
    private readonly IAccountRepository _accountRepository;

    private readonly List<AccountData> _accountRepositoryList = new();
    private readonly List<PlayerData> _dbPlayerRepositoryList = new();
    private readonly ICacheManager _cacheManager;

    public DataTests(ITestOutputHelper outputHelper)
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
            .Replace(new ServiceDescriptor(typeof(IDbPlayerRepository), _ =>
            {
                var mock = new Mock<IDbPlayerRepository>();
                mock.Setup(x => x.GetPlayerAsync(It.IsAny<Guid>())).ReturnsAsync((Guid id) =>
                    _dbPlayerRepositoryList.FirstOrDefault(x => x.Id == id));
                mock.Setup(x => x.CreateAsync(It.IsAny<PlayerData>())).Callback<PlayerData>(account => {_dbPlayerRepositoryList.Add(account);});
                mock.Setup(x => x.DeletePlayerAsync(It.IsAny<PlayerData>())).Callback<PlayerData>(account => {_dbPlayerRepositoryList.Remove(account);});
                return mock.Object;
            }, ServiceLifetime.Singleton))
            .Replace(new ServiceDescriptor(typeof(ICacheManager), typeof(MockedCacheManager), ServiceLifetime.Singleton))
            .BuildServiceProvider();
        _playerManager = services.GetRequiredService<IPlayerManager>();
        _accountManager = services.GetRequiredService<IAccountManager>();
        _passwordHasher = services.GetRequiredService<IPasswordHasher>();
        _accountRepository = services.GetRequiredService<IAccountRepository>();
        _dbPlayerRepository = services.GetRequiredService<IDbPlayerRepository>();
        _cacheManager = services.GetRequiredService<ICacheManager>();
    }

    [Fact]
    public async Task CreateAccount()
    {
        var account = await _accountManager.CreateAsync("testificate", "testificate", "some@gmail.com", "1234567");

        account.Should().BeEquivalentTo(new AccountData
        {
            AccountStatus = new AccountStatusData
            {
                Description = "",
                Id = 1,
                AllowLogin = true,
                ClientStatus = "OK"
            },
            Status = 1,
            Email = "some@gmail.com",
            Username = "testificate",
            DeleteCode = "1234567",
            LastLogin = null
        }, cfg =>
        {
            return cfg
                .Excluding(x => x.Password)
                .Excluding(x => x.Id);
        });

        account.Id.Should().NotBeEmpty();
        Assert.True(_passwordHasher.VerifyHash(account.Password, "testificate"));

        var dbAccount = await _accountRepository.FindByIdAsync(account.Id);
        dbAccount.Should().BeEquivalentTo(account);
    }

    [Fact]
    public async Task CreateAccount_DuplicateUserName()
    {
        var accountRepositoryMock = new Mock<IAccountRepository>();
        accountRepositoryMock.Setup(x => x.FindByNameAsync("testificate"))
            .ReturnsAsync(() => new AccountData());
        var accountManager = new AccountManager(accountRepositoryMock.Object, _passwordHasher);
        await Assert.ThrowsAsync<InvalidOperationException>(() => accountManager.CreateAsync("testificate", "testificate", "some@gmail.com", "1234567"));
    }

    [Fact]
    public async Task CreateCharacter()
    {
        var account = await _accountManager.CreateAsync("testificate", "testificate", "some@gmail.com", "1234567"); 
        var player = await _playerManager.CreateAsync(account.Id, "Testificate", 1, 1);

        player.Should().BeEquivalentTo(new PlayerData
        {
            AccountId = account.Id,
            Name = "Testificate",
            PlayerClass = 1,
            Ht = 4,
            PositionX = 958870,
            PositionY = 272788
        }, cfg => cfg.Excluding(x => x.Id));
        player.Id.Should().NotBeEmpty();

        var dbPlayer = await _dbPlayerRepository.GetPlayerAsync(player.Id);
        dbPlayer.Should().BeEquivalentTo(player);

        var playerKey = $"player:{player.Id.ToString()}";
        ((MockedCacheManager)_cacheManager)._cache.Should().HaveCount(1).And.ContainKey(playerKey);
        ((MockedCacheManager)_cacheManager)._cache[playerKey].Should().BeEquivalentTo(player);
    }

    [Fact]
    public async Task DeleteCharacter()
    {
        var account = await _accountManager.CreateAsync("testificate", "testificate", "some@gmail.com", "1234567"); 
        var player = await _playerManager.CreateAsync(account.Id, "Testificate", 1, 1);
        await _playerManager.DeletePlayerAsync(player);

        ((MockedCacheManager)_cacheManager)._cache.Should().BeEmpty();
        _dbPlayerRepositoryList.Should().BeEmpty();
    }
}