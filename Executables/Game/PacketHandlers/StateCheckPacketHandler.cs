using System.Threading;
using System.Threading.Tasks;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers;

public class StateCheckPacketHandler : IGamePacketHandler<StateCheckPacket>
{
    public async Task ExecuteAsync(GamePacketContext<StateCheckPacket> ctx, CancellationToken token = default)
    {
        await ctx.Connection.Send(new ServerStatusPacket {
            Statuses = new [] {
                new ServerStatus {
                    Port = 13001,
                    Status = 1
                }
            },
            IsSuccess = 1
        });
    }
}