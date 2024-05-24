using QuantumCore.Game.Packets.Shop;

namespace QuantumCore.Game.PacketHandlers.Game;

[PacketHandler(typeof(ShopBuy))]
public class ShopBuyHandler
{
    public void Execute(GamePacketContext ctx, ShopBuy packet)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return;
        }

        player.Shop?.Buy(player, packet.Position, packet.Count);
    }
}