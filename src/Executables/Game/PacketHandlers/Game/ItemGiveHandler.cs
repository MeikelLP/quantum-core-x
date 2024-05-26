using Microsoft.Extensions.Logging;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Game;

[PacketHandler(typeof(ItemGive))]
public class ItemGiveHandler
{
    private readonly ILogger<ItemGiveHandler> _logger;

    public ItemGiveHandler(ILogger<ItemGiveHandler> logger)
    {
        _logger = logger;
    }

    public void Execute(GamePacketContext ctx, ItemGive packet)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return;
        }

        var entity = player.Map?.GetEntity(packet.TargetVid);
        if (entity == null)
        {
            _logger.LogDebug("Ignore item give to non existing entity");
            return;
        }

        var item = player.GetItem(packet.Window, packet.Position);
        if (item == null)
        {
            return;
        }

        _logger.LogInformation("Item give to {Entity}", entity);
        await GameEventManager.OnNpcGive(entity.EntityClass, player, item);
    }
}