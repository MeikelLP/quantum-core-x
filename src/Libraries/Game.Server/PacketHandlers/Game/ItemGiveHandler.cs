using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Packets;
using QuantumCore.API.PluginTypes;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ItemGiveHandler : IGamePacketHandler<ItemGive>
{
    private readonly ILogger<ItemGiveHandler> _logger;

    public ItemGiveHandler(ILogger<ItemGiveHandler> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync(GamePacketContext<ItemGive> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player is null)
        {
            ctx.Connection.Close();
            return;
        }

        var entity = player.Map?.GetEntity(ctx.Packet.TargetVid);
        if (entity is null)
        {
            _logger.LogDebug("Ignore item give to non existing entity");
            return;
        }

        var item = player.GetItem(ctx.Packet.Window, ctx.Packet.Position);
        if (item is null)
        {
            return;
        }

        _logger.LogInformation("Item give to {Entity}", entity);
        await GameEventManager.OnNpcGiveAsync(entity.EntityClass, player, item);
    }
}