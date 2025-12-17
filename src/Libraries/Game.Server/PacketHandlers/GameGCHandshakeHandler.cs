using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Core.Timekeeping;
using QuantumCore.API.PluginTypes;
using QuantumCore.Core.Packets;

namespace QuantumCore.Game.PacketHandlers;

public class GameGcHandshakeHandler : IGamePacketHandler<GcHandshake>
{
    public Task ExecuteAsync(GamePacketContext<GcHandshake> ctx, CancellationToken token = default)
    {
        ctx.Connection.HandleHandshake(new GcHandshakeData
        {
            Delta = TimeSpan.FromMilliseconds(ctx.Packet.Delta),
            Handshake = ctx.Packet.Handshake,
            Time = new ServerTimestamp(TimeSpan.FromMilliseconds(ctx.Packet.Time))
        });
        return Task.CompletedTask;
    }
}
