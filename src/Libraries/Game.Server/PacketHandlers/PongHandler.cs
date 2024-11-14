using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Caching;
using QuantumCore.Game.Packets;
using QuantumCore.Networking;

namespace QuantumCore.Game.PacketHandlers;

public class PongHandler : IGamePacketHandler<Pong>
{
    private readonly ILogger<PongHandler> _logger;
    private readonly ICacheManager _cacheManager;

    public PongHandler(ILogger<PongHandler> logger, ICacheManager cacheManager)
    {
        _logger = logger;
        _cacheManager = cacheManager;
    }

    public async Task ExecuteAsync(GamePacketContext<Pong> ctx, CancellationToken token = default)
    {
        var expiration = TimeSpan.FromSeconds(NetworkingConstants.PingIntervalInSeconds + 1);

        _logger.LogDebug("Received pong from {Username}", ctx.Connection.AccountId);
        var activeToken = await _cacheManager.Shared.Get<uint>($"account:token:{ctx.Connection.AccountId}");

        await _cacheManager.Shared.Expire($"account:token:{ctx.Connection.AccountId}", expiration);
        await _cacheManager.Server.Expire($"account:{ctx.Connection.AccountId}:game:select:selected-player",
            expiration);
        await _cacheManager.Server.Expire($"token:{activeToken}", expiration);

        // Send a Ping packet to acknowledge the Pong. This won't be responded to with a Pong by the client
        ctx.Connection.Send(new Ping());
    }
}
