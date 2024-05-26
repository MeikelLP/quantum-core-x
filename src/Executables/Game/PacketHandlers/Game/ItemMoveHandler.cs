using Microsoft.Extensions.Logging;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Game;

[PacketHandler(typeof(ItemMove))]
public class ItemMoveHandler
{
    private readonly ILogger<ItemMoveHandler> _logger;

    public ItemMoveHandler(ILogger<ItemMoveHandler> logger)
    {
        _logger = logger;
    }

    public void Execute(GamePacketContext ctx, ItemMove packet)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return;
        }

        _logger.LogDebug("Move item from {FromWindow},{FromPosition} to {ToWindow},{ToPosition}", packet.FromWindow,
            packet.FromPosition, packet.ToWindow, packet.ToPosition);

        // Get moved item
        var item = player.GetItem(packet.FromWindow, packet.FromPosition);
        if (item == null)
        {
            _logger.LogDebug("Moved item not found!");
            return;
        }

        // Check if target space is available
        if (player.IsSpaceAvailable(item, packet.ToWindow, packet.ToPosition))
        {
            // remove from old space
            player.RemoveItem(item);

            // place item
            player.SetItem(item, packet.ToWindow, packet.ToPosition);

            // send item movement to client
            player.SendRemoveItem(packet.FromWindow, packet.FromPosition);
            player.SendItem(item);
        }
    }
}