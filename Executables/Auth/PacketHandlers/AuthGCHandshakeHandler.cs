using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.PluginTypes;
using QuantumCore.Core.Packets;

namespace QuantumCore.Auth.PacketHandlers;

public class AuthGCHandshakeHandler : IAuthPacketHandler<GCHandshake>
{
    public async Task ExecuteAsync(AuthPacketContext<GCHandshake> ctx, CancellationToken token = default)
    {
        await ctx.Connection.HandleHandshake(new GCHandshakeData {
            Delta = ctx.Packet.Delta,
            Handshake = ctx.Packet.Handshake,
            Time = ctx.Packet.Time
        });
    }
}