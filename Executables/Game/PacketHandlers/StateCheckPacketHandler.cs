using System.Threading;
using System.Threading.Tasks;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers;

public class StateCheckPacketHandler : IGamePacketHandler<StateCheckPacket>
{
    public Task ExecuteAsync(GamePacketContext<StateCheckPacket> ctx, CancellationToken token = default)
    {
        ctx.Connection.Send(new ServerStatusPacket {
            Statuses = new [] {
                new ServerStatus {
                    Port = 13001,
                    Status = 1
                }
            },
            IsSuccess = 1
        });
        return Task.CompletedTask;
    }
}
