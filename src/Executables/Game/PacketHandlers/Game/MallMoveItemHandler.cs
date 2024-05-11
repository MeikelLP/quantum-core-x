using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets.Mall;

namespace QuantumCore.Game.PacketHandlers.Game;

public class MallMoveItemHandler : IGamePacketHandler<MallMoveItem>
{
    public Task ExecuteAsync(GamePacketContext<MallMoveItem> ctx, CancellationToken token = default)
    {
        /*
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return Task.CompletedTask;
        }

        var item = player.Mall.GetItemAt(ctx.Packet.ItemPosition);
        if (item == null)
        {
            ctx.Connection.Close();
            return Task.CompletedTask;
        }
        
        var itemInstance = new ItemInstance
        {
            Id = Guid.NewGuid(),
            Count = 1,
            PlayerId = player.Player.Id,
            ItemId = 11210
        };
        
        player.Inventory.PlaceItem()

        player.Mall.MoveItem(ctx.Packet.ItemPosition, ctx.Packet.WindowPosition);
        */
        return Task.CompletedTask;
    }
}
