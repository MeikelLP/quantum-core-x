using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ItemPickupHandler : IGamePacketHandler<ItemPickup>
{
    public Task ExecuteAsync(GamePacketContext<ItemPickup> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return Task.CompletedTask;
        }

        var entity = player.Map?.GetEntity(ctx.Packet.Vid);
        if (entity is not GroundItem groundItem)
        {
            // we can only pick up ground items
            return Task.CompletedTask;
        }
        
        player.Pickup(groundItem);
        return Task.CompletedTask;
    }
}
