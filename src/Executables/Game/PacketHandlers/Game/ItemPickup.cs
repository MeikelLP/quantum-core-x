using QuantumCore.Game.Packets;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.PacketHandlers.Game;

[PacketHandler(typeof(ItemPickup))]
public class ItemPickupHandler
{
    public void Execute(GamePacketContext ctx, ItemPickup packet)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return;
        }

        var entity = player.Map?.GetEntity(packet.Vid);
        if (entity is not GroundItem groundItem)
        {
            // we can only pick up ground items
            return;
        }

        player.Pickup(groundItem);
    }
}