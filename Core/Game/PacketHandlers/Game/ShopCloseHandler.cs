using System.Threading;
using System.Threading.Tasks;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets.Shop;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ShopCloseHandler : IPacketHandler<ShopClose>
{
    public Task ExecuteAsync(PacketContext<ShopClose> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return Task.CompletedTask;
        }
            
        player.Shop?.Close(player);

        return Task.CompletedTask;
    }
}