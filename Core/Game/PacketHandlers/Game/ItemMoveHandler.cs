using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QuantumCore.Core.Networking;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ItemMoveHandler : IPacketHandler<ItemMove>
{
    private readonly ILogger<ItemMoveHandler> _logger;

    public ItemMoveHandler(ILogger<ItemMoveHandler> logger)
    {
        _logger = logger;
    }
        
    public async Task ExecuteAsync(PacketContext<ItemMove> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return;
        }
            
        _logger.LogDebug("Move item from {FromWindow},{FromPosition} to {ToWindow},{ToPosition}", ctx.Packet.FromWindow, ctx.Packet.FromPosition, ctx.Packet.ToWindow, ctx.Packet.ToPosition);

        // Get moved item
        var item = player.GetItem(ctx.Packet.FromWindow, ctx.Packet.FromPosition);
        if (item == null)
        {
            _logger.LogDebug("Moved item not found!");
            return;
        }

        // Check if target space is available
        if (player.IsSpaceAvailable(item, ctx.Packet.ToWindow, ctx.Packet.ToPosition))
        {
            // remove from old space
            await player.RemoveItem(item);
                
            // place item
            await player.SetItem(item, ctx.Packet.ToWindow, ctx.Packet.ToPosition);

            // send item movement to client
            await player.SendRemoveItem(ctx.Packet.FromWindow, ctx.Packet.FromPosition);
            await player.SendItem(item);
        }
    }
}