using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QuantumCore.Core.Networking;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Game;

public class TargetChangeHandler : IPacketHandler<TargetChange>
{
    private readonly ILogger<TargetChange> _logger;

    public TargetChangeHandler(ILogger<TargetChange> logger)
    {
        _logger = logger;
    }
        
    public async Task ExecuteAsync(PacketContext<TargetChange> ctx, CancellationToken token = default)
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