using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets.Shop;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ShopBuyHandler : IGamePacketHandler<ShopBuy>
{
    public Task ExecuteAsync(GamePacketContext<ShopBuy> ctx, CancellationToken token = default)
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