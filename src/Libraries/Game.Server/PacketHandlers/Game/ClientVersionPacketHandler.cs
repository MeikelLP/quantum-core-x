using Microsoft.Extensions.Logging;
using QuantumCore.PluginTypes;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ClientVersionPacketHandler : IGamePacketHandler<Packets.Version>
{
    private readonly ILogger<ClientVersionPacketHandler> _logger;

    public ClientVersionPacketHandler(ILogger<ClientVersionPacketHandler> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(GamePacketContext<Packets.Version> ctx, CancellationToken token = default)
    {
        _logger.LogInformation("Received client version: {Name} {Timestamp}", ctx.Packet.ExecutableName,
            ctx.Packet.Timestamp);

        return Task.CompletedTask;
    }
}
