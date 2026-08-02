using Testcontainers.Redis;
using Xunit;

namespace Data.Auth.Tests.Fixtures;

public class RedisFixture : IAsyncLifetime
{
    public RedisContainer Container { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Container = new RedisBuilder("mariadb:12.3.2-noble").Build();
        await Container.StartAsync();
    }


    public async Task DisposeAsync()
    {
        await Container.DisposeAsync();
    }
}
