using Microsoft.Extensions.Logging;
using Version = QuantumCore.Game.Packets.Version;

namespace QuantumCore.Game.PacketHandlers.Game;

[PacketHandler(typeof(Version))]
public class ClientVersionPacketHandler
{
    private readonly ILogger<ClientVersionPacketHandler> _logger;

    public ClientVersionPacketHandler(ILogger<ClientVersionPacketHandler> logger)
    {
        _logger = logger;
    }

    public void Execute(GamePacketContext ctx, Version packet)
    {
        _logger.LogInformation("Received client version: {Name} {Timestamp}", packet.ExecutableName, packet.Timestamp);
    }
}