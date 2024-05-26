using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers;

[PacketHandler(typeof(StateCheckPacket))]
public class StateCheckPacketHandler
{
    public void Execute(GamePacketContext ctx, StateCheckPacket packet)
    {
        ctx.Connection.Send(new ServerStatusPacket(
            [
                new ServerStatus(
                    13001,
                    1
                )
            ],
            1
        ));
    }
}