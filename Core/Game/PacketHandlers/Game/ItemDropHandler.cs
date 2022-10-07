using System.Threading;
using System.Threading.Tasks;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ItemDropHandler : IPacketHandler<ItemDrop>
{
    public async Task ExecuteAsync(PacketContext<ItemDrop> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return;
        }

        if (ctx.Packet.Gold > 0)
        {
            // We're dropping gold...
            await player.DropGold(ctx.Packet.Gold);
        }
        else
        {
            // We're dropping an item...
            var item = player.GetItem(ctx.Packet.Window, ctx.Packet.Position);
            if (item == null)
            {
                return; // Item slot is empty
            }

            await player.DropItem(item, ctx.Packet.Count);
        }
    }
}