using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Game;

[PacketHandler(typeof(ItemDrop))]
public class ItemDropHandler
{
    public void Execute(GamePacketContext ctx, ItemDrop packet)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return;
        }

        if (packet.Gold > 0)
        {
            // We're dropping gold...
            player.DropGold(packet.Gold);
        }
        else
        {
            // We're dropping an item...
            var item = player.GetItem(packet.Window, packet.Position);
            if (item == null)
            {
                return; // Item slot is empty
            }

            player.DropItem(item, packet.Count);
        }
    }
}