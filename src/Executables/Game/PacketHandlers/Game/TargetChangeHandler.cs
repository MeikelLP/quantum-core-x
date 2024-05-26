using Microsoft.Extensions.Logging;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Game;

[PacketHandler(typeof(TargetChange))]
public class TargetChangeHandler
{
    private readonly ILogger<TargetChangeHandler> _logger;

    public TargetChangeHandler(ILogger<TargetChangeHandler> logger)
    {
        _logger = logger;
    }

    public void Execute(GamePacketContext ctx, TargetChange packet)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            _logger.LogWarning("Target Change without having a player instance");
            ctx.Connection.Close();
            return;
        }

        var entity = player.Map?.GetEntity(packet.TargetVid);
        if (entity == null)
        {
            return;
        }

        player.Target?.TargetedBy.Remove(player);
        player.Target = entity;
        entity.TargetedBy.Add(player);
        player.SendTarget();
    }
}