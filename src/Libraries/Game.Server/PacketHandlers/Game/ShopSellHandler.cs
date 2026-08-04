using QuantumCore.API;
using QuantumCore.API.Packets.Shop;
using QuantumCore.API.PluginTypes;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ShopSellHandler : IGamePacketHandler<ShopSell>
{
    public async Task ExecuteAsync(GamePacketContext<ShopSell> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player is null)
        {
            ctx.Connection.Close();
            return;
        }

        if (player.Shop is not null)
        {
            await player.Shop.SellAsync(player, ctx.Packet.Position);
        }
    }
}