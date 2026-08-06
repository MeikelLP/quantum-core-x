using System.Net;
using AwesomeAssertions;
using CommandLine;
using Game.Commands.Tests.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using QuantumCore.API;
using QuantumCore.API.Game.World;
using QuantumCore.API.Packets;
using QuantumCore.Game.Commands;
using QuantumCore.Game.Persistence.Entities;

namespace Game.Commands.Tests;

public class StrictCommandManagerTests
{
    private readonly ICommandManager _commandManager;
    private readonly IGameConnection _connection;
    private readonly List<string> _chatInfos = new();

    public StrictCommandManagerTests()
    {
        var services = new ServiceCollection()
            .AddSingleton(_ =>
            {
                var player = Substitute.For<IPlayerEntity>();
                player.Groups.Returns([PermGroup.OperatorGroup]);
                var conn = Substitute.For<IGameConnection>();
                conn.When(x => x.Send(Arg.Any<ChatOutcoming>()))
                    .Do(info => _chatInfos.Add(info.Arg<ChatOutcoming>()!.Message!));
                conn.Player.Returns(player);
                conn.BoundIpAddress.Returns(IPAddress.Loopback);
                return conn;
            })
            .AddSingleton<IConfiguration>(_ => new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { { "Game:Commands:StrictMode", "true" } })
                .Build())
            .AddGameCommands()
            .AddQuantumCoreTestLogger()
            .BuildServiceProvider();
        _commandManager = services.GetRequiredService<ICommandManager>();
        _connection = services.GetRequiredService<IGameConnection>();
    }

    [Fact]
    public async Task StrictModeAsync()
    {
        var ex = await Assert.ThrowsAsync<CommandHandlerNotFoundException>(() =>
            _commandManager.HandleAsync(_connection, "/some_command"));
        ex.Command.Should().BeEquivalentTo("some_command");
    }

    [Fact]
    public async Task ValidateCommand_ArgumentMissingAsync()
    {
        _commandManager.Register(typeof(SetJobCommand).Namespace!, typeof(SetJobCommand).Assembly);
        var ex = await Assert.ThrowsAsync<CommandValidationException>(() =>
            _commandManager.HandleAsync(_connection, "/setjob"));
        ex.Command.Should().BeEquivalentTo("setjob");
        ex.Errors.Should().BeEquivalentTo([nameof(MissingRequiredOptionError)]);
    }

    [Fact]
    public async Task ValidateCommand_ArgumentInvalidTypeAsync()
    {
        _commandManager.Register(typeof(SetJobCommand).Namespace!, typeof(SetJobCommand).Assembly);
        var ex = await Assert.ThrowsAsync<CommandValidationException>(() =>
            _commandManager.HandleAsync(_connection, "/setjob a"));
        ex.Command.Should().BeEquivalentTo("setjob");
        ex.Errors.Should().BeEquivalentTo([nameof(BadFormatConversionError), nameof(MissingRequiredOptionError)]);
    }
}