using Testcontainers.Redis;

namespace Game.Tests.Fixtures;

public class RedisFixture : IAsyncLifetime
{
    public RedisContainer Container { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        Container = new RedisBuilder("valkey/valkey:9.1.1-alpine3.24").Build();
        await Container.StartAsync();
    }


    public async ValueTask DisposeAsync()
    {
        await Container.DisposeAsync();
    }
}