using QuantumCore.Packets;
using QuantumCore.Game.Types;
using QuantumCore.PluginTypes;

namespace QuantumCore.Game.PacketHandlers;

public class StateCheckPacketHandler : IGamePacketHandler<StateCheckPacket>
{
    public Task ExecuteAsync(GamePacketContext<StateCheckPacket> ctx, CancellationToken token = default)
    {
        ctx.Connection.Send(new ServerStatusPacket
        {
            Statuses =
            [
                new ServerStatus
                {
                    Port = 13001, // TODO: this should be based on `Hosting:Port` config
                    Status = EServerStatus.ONLINE
                }
            ],
            IsSuccess = 1
        });
        return Task.CompletedTask;
    }
}