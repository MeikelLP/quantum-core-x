using Testcontainers.Redis;

namespace Game.Tests.Fixtures;

public class RedisFixture : IAsyncLifetime
{
    public RedisContainer Container { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Container = new RedisBuilder("valkey:9.1.1-alpne3.24").Build();
        await Container.StartAsync();
    }


    public async Task DisposeAsync()
    {
        await Container.DisposeAsync();
    }
}
