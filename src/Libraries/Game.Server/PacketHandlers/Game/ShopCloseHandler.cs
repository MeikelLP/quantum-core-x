using QuantumCore.API;
using QuantumCore.API.Packets.Shop;
using QuantumCore.API.PluginTypes;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ShopCloseHandler : IGamePacketHandler<ShopClose>
{
    public Task ExecuteAsync(GamePacketContext<ShopClose> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player is null)
        {
            ctx.Connection.Close();
            return Task.CompletedTask;
        }

        player.Shop?.Close(player);

        return Task.CompletedTask;
    }
}
