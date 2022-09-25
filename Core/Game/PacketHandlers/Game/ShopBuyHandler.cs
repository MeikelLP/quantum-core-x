using System.Threading;
using System.Threading.Tasks;
using QuantumCore.API;
using QuantumCore.Core.Networking;
using QuantumCore.Game.Packets.Shop;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ShopBuyHandler : IPacketHandler<ShopBuy>
{
    public Task ExecuteAsync(PacketContext<ShopBuy> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();

            return Task.CompletedTask;
        }

        player.Shop?.Buy(player, ctx.Packet.Position, ctx.Packet.Count);

        return Task.CompletedTask;
    }
}