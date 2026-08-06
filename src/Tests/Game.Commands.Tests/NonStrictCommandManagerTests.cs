using System.Net;
using AwesomeAssertions;
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

public class NonStrictCommandManagerTests
{
    private readonly ICommandManager _commandManager;
    private readonly IGameConnection _connection;
    private readonly List<string> _chatInfos = new();

    public NonStrictCommandManagerTests()
    {
        var services = new ServiceCollection()
            .AddSingleton(_ =>
            {
                var conn = Substitute.For<IGameConnection>();
                var player = Substitute.For<IPlayerEntity>();
                player.Groups.Returns([PermGroup.OperatorGroup]);
                player.Connection.Returns(conn);
                conn.When(x => x.Send(Arg.Any<ChatOutcoming>()))
                    .Do(info => _chatInfos.Add(info.Arg<ChatOutcoming>()!.Message));
                conn.Player.Returns(player);
                conn.BoundIpAddress.Returns(IPAddress.Loopback);
                return conn;
            })
            .AddSingleton<IConfiguration>(_ => new ConfigurationBuilder().Build())
            .AddGameCommands()
            .AddQuantumCoreTestLogger()
            .BuildServiceProvider();
        _commandManager = services.GetRequiredService<ICommandManager>();
        _connection = services.GetRequiredService<IGameConnection>();
    }

    [Fact]
    public async Task InvalidCommand_NonStrictModeAsync()
    {
        _commandManager.Register(typeof(SetJobCommand).Namespace!, typeof(SetJobCommand).Assembly);
        await _commandManager.HandleAsync(_connection, "/setjob b");

        _chatInfos.Should().BeEquivalentTo(["Command validation failed:", "  value pos. 0    Required."]);
    }
}