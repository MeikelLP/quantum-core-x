using System.Net;
using Microsoft.Extensions.Logging;
using NSubstitute;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Guild;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.Types.Players;
using QuantumCore.API.Game.World;
using QuantumCore.Auth.Cache;
using QuantumCore.Caching;
using QuantumCore.Game;
using QuantumCore.Game.PacketHandlers;
using QuantumCore.Game.Packets;

namespace Game.Tests;

public class TokenLoginHandlerTests
{
    [Theory]
    [InlineData("0.0.0.0", "127.0.0.1", "127.0.0.1")]
    [InlineData("0.0.0.0", "172.16.165.1", "172.16.165.1")]
    [InlineData("127.0.0.1", "127.0.0.1", "127.0.0.1")]
    [InlineData("198.51.100.10", "198.51.100.10", "198.51.100.10")]
    [InlineData("93.184.216.34", "93.184.216.34", "93.184.216.34")]
    public async Task ExecuteAsync_AdvertisesExpectedCharacterHost(
        string mapHostIp,
        string connectionInterfaceIp,
        string expectedAdvertisedIp)
    {
        var parsedMapHostIp = IPAddress.Parse(mapHostIp);
        var parsedConnectionInterfaceIp = IPAddress.Parse(connectionInterfaceIp);
        var parsedExpectedAdvertisedIp = IPAddress.Parse(expectedAdvertisedIp);

        var harness = new TokenLoginHandlerTestHarness(
            parsedMapHostIp,
            parsedConnectionInterfaceIp);

        var characters = await harness.ExecuteAsync();

        var expectedIpInt = BitConverter.ToInt32(parsedExpectedAdvertisedIp.GetAddressBytes());
        var actualIpInt = characters.CharacterList[0].Ip;

        Assert.Equal(expectedIpInt, actualIpInt);
    }

    private sealed class TokenLoginHandlerTestHarness
    {
        private readonly IGameConnection _connection;
        private readonly TokenLoginHandler _handler;
        private readonly TokenLogin _tokenLogin;
        private Characters? _characters;

        public TokenLoginHandlerTestHarness(IPAddress mapHostIp, IPAddress connectionInterfaceIp)
        {
            var cacheManager = Substitute.For<ICacheManager>();
            var world = Substitute.For<IWorld>();
            var playerManager = Substitute.For<IPlayerManager>();
            var guildManager = Substitute.For<IGuildManager>();
            var serverStore = Substitute.For<IRedisStore>();
            var sharedStore = Substitute.For<IRedisStore>();

            cacheManager.Server.Returns(serverStore);
            cacheManager.Shared.Returns(sharedStore);

            const string Username = "test-user";
            var accountId = Guid.Parse("df2ce2b2-e1b9-46e4-9d62-991b8b590d1b");
            const uint TokenKey = 0xDEADBEEFu;
            var tokenCacheKey = $"token:{TokenKey}";
            var accountTokenKey = $"account:token:{accountId}";

            serverStore.Exists(tokenCacheKey).Returns(new ValueTask<long>(1));
            serverStore.Get<Token>(tokenCacheKey).Returns(new ValueTask<Token>(new Token
            {
                AccountId = accountId,
                Username = Username
            }));
            sharedStore.Get<uint>(accountTokenKey).Returns(new ValueTask<uint>(0));
            serverStore.Persist(tokenCacheKey).Returns(new ValueTask<long>(1));
            sharedStore.Expire(accountTokenKey, Arg.Any<TimeSpan>()).Returns(new ValueTask<long>(1));
            serverStore.Set(Arg.Any<string>(), Arg.Any<object>()).Returns(new ValueTask<string>("OK"));

            var player = new PlayerData
            {
                Id = 100,
                AccountId = accountId,
                Name = "TestPlayer",
                PlayerClass = EPlayerClassGendered.NinjaFemale,
                Slot = 0,
                PositionX = 100,
                PositionY = 200,
                Empire = EEmpire.Shinsoo
            };

            playerManager.GetPlayers(accountId).Returns(Task.FromResult(new[] {player}));
            guildManager.GetGuildForPlayerAsync(player.Id, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<GuildData?>(null));

            var coreHost = new CoreHost {_ip = mapHostIp, _port = 13000};
            world.GetMapHost(Arg.Any<int>(), Arg.Any<int>()).Returns(coreHost);

            var serverBase = Substitute.For<IServerBase>();
            serverBase.IpAddress.Returns(connectionInterfaceIp);
            serverBase.Port.Returns(coreHost._port);

            var phase = EPhase.Handshake;
            var assignedAccountId = (Guid?) null;
            var assignedUsername = string.Empty;

            var connection = Substitute.For<IGameConnection>();
            connection.Server.Returns(serverBase);
            connection.BoundIpAddress.Returns(connectionInterfaceIp);
            connection.Phase.Returns(_ => phase);
            connection.When(x => x.Phase = Arg.Any<EPhase>()).Do(ci => phase = ci.Arg<EPhase>());
            connection.AccountId.Returns(_ => assignedAccountId);
            connection.When(x => x.AccountId = Arg.Any<Guid?>()).Do(ci => assignedAccountId = ci.Arg<Guid?>());
            connection.Username.Returns(_ => assignedUsername);
            connection.When(x => x.Username = Arg.Any<string>()).Do(ci => assignedUsername = ci.Arg<string>());
            connection.Send(Arg.Do<Characters>(packet => _characters = packet));

            _connection = connection;
            _tokenLogin = new TokenLogin {Username = Username, Key = TokenKey};
            _handler = new TokenLoginHandler(Substitute.For<ILogger<TokenLoginHandler>>(), cacheManager, world, playerManager, guildManager);
        }

        public async Task<Characters> ExecuteAsync()
        {
            var context = new GamePacketContext<TokenLogin>
            {
                Packet = _tokenLogin,
                Connection = _connection
            };

            await _handler.ExecuteAsync(context);

            return _characters ?? throw new InvalidOperationException("Characters packet was not sent.");
        }
    }
}
