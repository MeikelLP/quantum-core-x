using QuantumCore.Core.Packets;
using QuantumCore.Models;
using QuantumCore.PluginTypes;

namespace QuantumCore.Game.PacketHandlers;

public class GameGcHandshakeHandler : IGamePacketHandler<GcHandshake>
{
    public Task ExecuteAsync(GamePacketContext<GcHandshake> ctx, CancellationToken token = default)
    {
        ctx.Connection.HandleHandshake(new GcHandshakeData
        {
            Delta = TimeSpan.FromMilliseconds(ctx.Packet.Delta),
            Handshake = ctx.Packet.Handshake,
            Time = TimeSpan.FromMilliseconds(ctx.Packet.Time)
        });
        return Task.CompletedTask;
    }
}
