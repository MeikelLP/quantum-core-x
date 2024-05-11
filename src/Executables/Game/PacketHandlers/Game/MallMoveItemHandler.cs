using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets.Mall;

namespace QuantumCore.Game.PacketHandlers.Game;

public class MallMoveItemHandler : IGamePacketHandler<MallMoveItem>
{
    public Task ExecuteAsync(GamePacketContext<MallMoveItem> ctx, CancellationToken token = default)
    {
        // todo
        return Task.CompletedTask;
    }
}
