using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.Packets;
using QuantumCore.Core.Networking;
using QuantumCore.Game.Events;
using QuantumCore.Game.Types;
using QuantumCore.Game.World;
using QuantumCore.Networking;
using QuantumCore.Timekeeping;

namespace QuantumCore.Game;

public class GameConnection : Connection, IGameConnection
{
    private readonly IWorld _world;
    private readonly ICacheManager _cacheManager;
    public IServerBase Server { get; }
    public Guid? AccountId { get; set; }
    public string Username { get; set; } = "";
    public IPlayerEntity? Player { get; set; }


    public GameConnection(IServerBase server, TcpClient client, ILogger<GameConnection> logger,
        IWorld world,
        [FromKeyedServices(HostingOptions.MODE_GAME)]
        IPacketReader packetReader,
        ICacheManager cacheManager)
        : base(logger, packetReader)
    {
        _world = world;
        Server = server;
        _cacheManager = cacheManager;
        Init(client);
    }

    protected override void OnHandshakeFinished()
    {
        Server.CallConnectionListener(this);
        var pingInterval = TimeSpan.FromSeconds(NetworkingConstants.PingIntervalInSeconds);
        var ping = new Ping();
        EventSystem.EnqueueEvent(() =>
        {
            Send(ping);
            return pingInterval;
        }, pingInterval);
    }

    protected override async Task OnCloseAsync(bool expected = true)
    {
        if (Player is not null)
        {
            if (expected)
            {
                _world.DespawnEntity(Player);
            }
            else
            {
                if (Phase is EPhase.GAME or EPhase.LOADING)
                {
                    await Player.CalculatePlayedTimeAsync();
                }

                // In case of unexpected disconnection, we need to save the player's state
                if (Phase is EPhase.GAME or EPhase.LOADING or EPhase.SELECT)
                {
                    await _world.DespawnPlayerAsync(Player);
                }
            }


            await _cacheManager.Server.DelAllAsync($"player:{Player!.Player.Id}");
            await _cacheManager.DelAsync($"account:token:{Player.Player.AccountId}");
        }

        Server.RemoveConnection(this);

        // todo enable expiry on auth token
    }

    protected override async Task OnReceiveAsync(IPacketSerializable packet)
    {
        await Server.CallListenerAsync(this, packet);
    }

    protected override ServerClock GetClock()
    {
        return Server.Clock;
    }
}