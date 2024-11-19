using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets.Shop;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ShopCloseHandler : IGamePacketHandler<ShopClose>
{
    public Task ExecuteAsync(GamePacketContext<ShopClose> ctx, CancellationToken token = default)
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