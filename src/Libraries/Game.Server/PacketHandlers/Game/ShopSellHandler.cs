using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets.Shop;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ShopSellHandler : IGamePacketHandler<ShopSell>
{
    public Task ExecuteAsync(GamePacketContext<ShopSell> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player is null)
        {
            ctx.Connection.Close();
            return Task.CompletedTask;
        }

        player.Shop?.Sell(player, ctx.Packet.Position);

        return Task.CompletedTask;
    }
}
