using QuantumCore.API;
using QuantumCore.API.Packets;
using QuantumCore.API.PluginTypes;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ItemDropHandler : IGamePacketHandler<ItemDrop>
{
    public async Task ExecuteAsync(GamePacketContext<ItemDrop> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player is null)
        {
            ctx.Connection.Close();
            return;
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
            if (item is null)
            {
                return; // Item slot is empty
            }

            await player.DropItemAsync(item, ctx.Packet.Count);
        }
    }
}