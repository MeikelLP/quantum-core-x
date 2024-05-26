using QuantumCore.Game.Packets.Shop;

namespace QuantumCore.Game.PacketHandlers.Game;

[PacketHandler(typeof(ShopSell))]
public class ShopSellHandler
{
    public void Execute(GamePacketContext ctx, ShopSell packet)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return;
        }

        player.Shop?.Sell(player, packet.Position);
    }
}