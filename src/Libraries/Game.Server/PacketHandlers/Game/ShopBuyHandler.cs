using QuantumCore.API;
using QuantumCore.API.Packets.Shop;
using QuantumCore.API.PluginTypes;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ShopBuyHandler : IGamePacketHandler<ShopBuy>
{
    public async Task ExecuteAsync(GamePacketContext<ShopBuy> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player is null)
        {
            ctx.Connection.Close();

            return;
        }

        if (player.Shop is not null)
        {
            await player.Shop.BuyAsync(player, ctx.Packet.Position, ctx.Packet.Count);
        }
    }
}