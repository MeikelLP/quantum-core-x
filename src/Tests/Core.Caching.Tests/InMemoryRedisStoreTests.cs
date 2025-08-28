using QuantumCore.Caching.InMemory;

namespace Single.Tests;

public class InMemoryRedisStoreTests
{
    [Theory]
    [InlineData("accounts:*", "accounts:.*")]
    [InlineData("*:perms:*", ".*:perms:.*")]
    [InlineData("*:perms", ".*:perms")]
    [InlineData("accounts.*", @"accounts\..*")]
    public void RedisPatternToRegex(string redis, string regexString)
    {
        var regex = InMemoryRedisStore.RedisPatternToRegex(redis);
        regex.ToString().Should().BeEquivalentTo(regexString);
    }
}
