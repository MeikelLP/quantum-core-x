using System.Threading;
using System.Threading.Tasks;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.PluginTypes;
using QuantumCore.Core.Packets;

namespace QuantumCore.Game.PacketHandlers;

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

public class GameGCHandshakeHandler : IGamePacketHandler<GCHandshake>
{
    public async Task ExecuteAsync(GamePacketContext<GCHandshake> ctx, CancellationToken token = default)
    {
        await ctx.Connection.HandleHandshake(new GCHandshakeData {
            Delta = ctx.Packet.Delta,
            Handshake = ctx.Packet.Handshake,
            Time = ctx.Packet.Time
        });
    }
}