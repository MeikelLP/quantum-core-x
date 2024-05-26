using QuantumCore.Game.Packets.Shop;

namespace QuantumCore.Game.PacketHandlers.Game;

[PacketHandler(typeof(ShopClose))]
public class ShopCloseHandler
{
    public void Execute(GamePacketContext ctx, ShopClose packet)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return;
        }

        player.Shop?.Close(player);
    }
}