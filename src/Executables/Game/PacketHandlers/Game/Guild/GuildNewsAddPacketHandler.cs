using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets.Guild;

namespace QuantumCore.Game.PacketHandlers.Game.Guild;

public class GuildNewsAddPacketHandler : IGamePacketHandler<GuildNewsAddPacket>
{
    private readonly ILogger<GuildNewsAddPacket> _logger;

    public GuildNewsAddPacketHandler(ILogger<GuildNewsAddPacket> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(GamePacketContext<GuildNewsAddPacket> ctx, CancellationToken token = default)
    {
        _logger.LogInformation("Received new guild news: {Value}", ctx.Packet.Value);

        return Task.CompletedTask;
    }
}