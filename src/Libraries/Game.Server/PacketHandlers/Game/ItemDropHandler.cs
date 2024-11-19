using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ItemDropHandler : IGamePacketHandler<ItemDrop>
{
    public Task ExecuteAsync(GamePacketContext<ItemDrop> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return Task.CompletedTask;
        }

        if (ctx.Packet.Gold > 0)
        {
            // We're dropping gold...
            player.DropGold(ctx.Packet.Gold);
        }
        else
        {
            // We're dropping an item...
            var item = player.GetItem(ctx.Packet.Window, ctx.Packet.Position);
            if (item == null)
            {
                return Task.CompletedTask; // Item slot is empty
            }

            player.DropItem(item, ctx.Packet.Count);
        }

        return Task.CompletedTask;
    }
}