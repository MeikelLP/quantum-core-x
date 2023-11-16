using Data.Tests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using QuantumCore;
using QuantumCore.API;
using QuantumCore.Auth.Persistence;
using QuantumCore.Caching;
using QuantumCore.Game.Extensions;
using Serilog;
using Testcontainers.MySql;
using Testcontainers.Redis;
using Xunit;
using Xunit.Abstractions;

namespace Data.Tests;

public class AccountManagerTests : IClassFixture<RedisFixture>, IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAccountManager _accountManager;
    private readonly IAccountRepository _accountRepository;
    private readonly ICacheManager _cacheManager;

    public AccountManagerTests(ITestOutputHelper outputHelper, RedisFixture redisFixture, DatabaseFixture databaseFixture)
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
            .Configure<DatabaseOptions>(opts =>
            {
                opts.Port = databaseFixture.Container.GetMappedPublicPort(MySqlBuilder.MySqlPort);
                opts.Password = DatabaseFixture.Password;
                opts.User = DatabaseFixture.UserName;
                opts.Database = DatabaseFixture.Database;
            })
            .Configure<CacheOptions>(opts =>
            {
                opts.Port = redisFixture.Container.GetMappedPublicPort(RedisBuilder.RedisPort);
            })
            .BuildServiceProvider();
        _accountManager = services.GetRequiredService<IAccountManager>();
        _passwordHasher = services.GetRequiredService<IPasswordHasher>();
        _accountRepository = services.GetRequiredService<IAccountRepository>();
        _cacheManager = services.GetRequiredService<ICacheManager>();
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
}
