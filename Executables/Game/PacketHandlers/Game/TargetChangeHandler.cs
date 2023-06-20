using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Game;

public class TargetChangeHandler : IGamePacketHandler<TargetChange>
{
    private readonly ILogger<TargetChangeHandler> _logger;

    public TargetChangeHandler(ILogger<TargetChangeHandler> logger)
    {
        _logger = logger;
    }
        
    public async Task ExecuteAsync(GamePacketContext<TargetChange> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            _logger.LogWarning("Target Change without having a player instance");
            ctx.Connection.Close();
            return;
        }

        var entity = player.Map.GetEntity(ctx.Packet.TargetVid);
        if (entity == null)
        {
            return;
        }

        player.Target?.TargetedBy.Remove(player);
        player.Target = entity;
        entity.TargetedBy.Add(player);
        await player.SendTarget();
    }
}