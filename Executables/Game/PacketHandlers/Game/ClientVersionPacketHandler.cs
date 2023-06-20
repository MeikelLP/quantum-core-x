using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using Version = QuantumCore.Game.Packets.Version;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ClientVersionPacketHandler : IGamePacketHandler<Version>
{
    private readonly ILogger<ClientVersionPacketHandler> _logger;

    public ClientVersionPacketHandler(ILogger<ClientVersionPacketHandler> logger)
    {
        _logger = logger;
    }
    
    public Task ExecuteAsync(GamePacketContext<Version> ctx, CancellationToken token = default)
    {
        _logger.LogInformation("Received client version: {Name} {Timestamp}", ctx.Packet.ExecutableName, ctx.Packet.Timestamp);

        return Task.CompletedTask;
    }
}