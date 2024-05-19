using FluentAssertions;
using Game.Commands.Tests.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using QuantumCore.API;
using QuantumCore.Game.Commands;
using Xunit.Abstractions;

namespace Game.Commands.Tests;

public class CommandManagerTests
{
    private readonly ICommandManager _commandManager;
    private readonly IGameConnection _connection;

    public CommandManagerTests(ITestOutputHelper outputHelper)
    {
        var services = new ServiceCollection()
            .AddSingleton(_ => Substitute.For<IGameConnection>())
            .AddSingleton<IConfiguration>(_ => new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"Game:Commands:StrictMode", "true"}
                })
                .Build())
            .AddGameCommands()
            .AddQuantumCoreTestLogger(outputHelper)
            .BuildServiceProvider();
        _commandManager = services.GetRequiredService<ICommandManager>();
        _connection = services.GetRequiredService<IGameConnection>();
    }

    [Fact]
    public async Task StrictMode()
    {
        var ex = await Assert.ThrowsAsync<CommandHandlerNotFoundException>(() =>
            _commandManager.Handle(_connection, "/some_command"));
        ex.Command.Should().BeEquivalentTo("some_command");
    }
}
