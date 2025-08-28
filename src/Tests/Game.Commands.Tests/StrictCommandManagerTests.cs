using AwesomeAssertions;
using CommandLine;
using Game.Commands.Tests.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using QuantumCore.API;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Commands;
using QuantumCore.Game.Persistence.Entities;
using Xunit.Abstractions;

namespace Game.Commands.Tests;

public class StrictCommandManagerTests
{
    private readonly ICommandManager _commandManager;
    private readonly IGameConnection _connection;
    private readonly List<string> _chatInfos = new();

    public StrictCommandManagerTests(ITestOutputHelper outputHelper)
    {
        var services = new ServiceCollection()
            .AddSingleton(_ =>
            {
                var player = Substitute.For<IPlayerEntity>();
                player.Groups.Returns([PermGroup.OperatorGroup]);
                player.When(x => x.SendChatInfo(Arg.Any<string>())).Do(info => _chatInfos.Add(info.Arg<string>()));
                var conn = Substitute.For<IGameConnection>();
                conn.Player.Returns(player);
                return conn;
            })
            .AddSingleton<IConfiguration>(_ => new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { { "Game:Commands:StrictMode", "true" } })
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

    [Fact]
    public async Task ValidateCommand_ArgumentMissing()
    {
        _commandManager.Register(typeof(SetJobCommand).Namespace!, typeof(SetJobCommand).Assembly);
        var ex = await Assert.ThrowsAsync<CommandValidationException>(() =>
            _commandManager.Handle(_connection, "/setjob"));
        ex.Command.Should().BeEquivalentTo("setjob");
        ex.Errors.Should().BeEquivalentTo([nameof(MissingRequiredOptionError)]);
    }

    [Fact]
    public async Task ValidateCommand_ArgumentInvalidType()
    {
        _commandManager.Register(typeof(SetJobCommand).Namespace!, typeof(SetJobCommand).Assembly);
        var ex = await Assert.ThrowsAsync<CommandValidationException>(() =>
            _commandManager.Handle(_connection, "/setjob a"));
        ex.Command.Should().BeEquivalentTo("setjob");
        ex.Errors.Should().BeEquivalentTo([nameof(BadFormatConversionError), nameof(MissingRequiredOptionError)]);
    }
}
