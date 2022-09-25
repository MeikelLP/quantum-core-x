using System.Threading;
using System.Threading.Tasks;
using QuantumCore.API;
using QuantumCore.Core.Networking;
using QuantumCore.Game.Packets.Shop;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ShopSellHandler : IPacketHandler<ShopSell>
{
    public Task ExecuteAsync(PacketContext<ShopSell> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return Task.CompletedTask;
        }

        player.Shop?.Sell(player, ctx.Packet.Position);

        return Task.CompletedTask;
    }
}