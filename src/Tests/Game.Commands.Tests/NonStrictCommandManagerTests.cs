using FluentAssertions;
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

public class NonStrictCommandManagerTests
{
    private readonly ICommandManager _commandManager;
    private readonly IGameConnection _connection;
    private readonly List<string> _chatInfos = new();

    public NonStrictCommandManagerTests(ITestOutputHelper outputHelper)
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
            .AddSingleton<IConfiguration>(_ => new ConfigurationBuilder().Build())
            .AddGameCommands()
            .AddQuantumCoreTestLogger(outputHelper)
            .BuildServiceProvider();
        _commandManager = services.GetRequiredService<ICommandManager>();
        _connection = services.GetRequiredService<IGameConnection>();
    }

    [Fact]
    public async Task InvalidCommand_NonStrictMode()
    {
        _commandManager.Register(typeof(SetJobCommand).Namespace!, typeof(SetJobCommand).Assembly);
        await _commandManager.Handle(_connection, "/setjob b");

        _chatInfos.Should().BeEquivalentTo(["Comannd validation failed:", "  value pos. 0    Required."]);
    }
}
