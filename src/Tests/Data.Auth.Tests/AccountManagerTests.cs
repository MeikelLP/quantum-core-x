using AwesomeAssertions;
using Data.Auth.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using QuantumCore;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.Auth.Persistence;
using QuantumCore.Auth.Persistence.Extensions;
using QuantumCore.Caching;
using QuantumCore.Caching.Extensions;
using Serilog;
using Testcontainers.Redis;
using Xunit;
using Xunit.Abstractions;

namespace Data.Auth.Tests;

public class AccountManagerTests : IClassFixture<RedisFixture>, IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAccountManager _accountManager;
    private readonly IAccountRepository _accountRepository;
    private readonly ICacheManager _cacheManager;
    private readonly AsyncServiceScope _scope;

    public AccountManagerTests(ITestOutputHelper outputHelper, RedisFixture redisFixture,
        DatabaseFixture databaseFixture)
    {
        var services = new ServiceCollection()
            .AddLogging(cfg => cfg
                .ClearProviders()
                .AddSerilog(new LoggerConfiguration()
                    .WriteTo.TestOutput(outputHelper)
                    .CreateLogger()))
            .AddAuthDatabase()
            .AddQuantumCoreCaching()
            .AddSingleton<IConfiguration>(new ConfigurationBuilder().Build())
            .Configure<DatabaseOptions>(HostingOptions.MODE_AUTH, opts =>
            {
                opts.Provider = DatabaseProvider.MYSQL;
                opts.ConnectionString = databaseFixture.Container.GetConnectionString();
            })
            .Configure<CacheOptions>(opts =>
            {
                opts.Port = redisFixture.Container.GetMappedPublicPort(RedisBuilder.RedisPort);
            })
            .BuildServiceProvider();
        _accountManager = services.GetRequiredService<IAccountManager>();
        _passwordHasher = services.GetRequiredService<IPasswordHasher>();
        _cacheManager = services.GetRequiredService<ICacheManager>();
        _scope = services.CreateAsyncScope();
        _accountRepository = _scope.ServiceProvider.GetRequiredService<IAccountRepository>();
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

        var db = _scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await db.AccountStatus.ExecuteDeleteAsync();
        await _scope.DisposeAsync();
    }

    [Fact]
    public async Task CreateAccount()
    {
        var account = await _accountManager.CreateAsync("testificate", "testificate", "some@gmail.com", "1234567");

        account.Should().BeEquivalentTo(
            new AccountData
            {
                AccountStatus =
                    new AccountStatusData
                    {
                        Description = "Default Status", Id = 1, AllowLogin = true, ClientStatus = "OK"
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
                    .Excluding(x => x.CreatedAt)
                    .Excluding(x => x.UpdatedAt)
                    .Excluding(x => x.Id);
            });

        account.CreatedAt.Should().NotBeSameDateAs(default);
        account.UpdatedAt.Should().NotBeSameDateAs(default);
        account.Id.Should().NotBeEmpty();
        _passwordHasher.VerifyHash(account.Password, "testificate").Should().BeTrue();

        var dbAccount = await _accountRepository.FindByIdAsync(account.Id);
        dbAccount.Should().BeEquivalentTo(account, cfg => cfg
            .Excluding(x => x.CreatedAt)
            .Excluding(x => x.UpdatedAt));
    }

    [Fact]
    public async Task CreateAccount_DuplicateUserName()
    {
        var accountRepositoryMock = Substitute.For<IAccountRepository>();
        accountRepositoryMock.FindByNameAsync("testificate").Returns(new AccountData());
        var accountManager = new AccountManager(accountRepositoryMock, _passwordHasher);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            accountManager.CreateAsync("testificate", "testificate", "some@gmail.com", "1234567"));
    }
}
