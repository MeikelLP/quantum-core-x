using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets.Guild;

namespace QuantumCore.Game.PacketHandlers.Game.Guild;

public class GuildRefreshCommentsHandler : IGamePacketHandler<GuildRefreshComments>
{
    public Task ExecuteAsync(GamePacketContext<GuildRefreshComments> ctx, CancellationToken token = default)
    {
        ctx.Connection.Send(new GuildNewsPacket
        {
            News =
            [
                new GuildNews
                {
                    NewsId = 1,
                    PlayerName = ctx.Connection.Player.Name,
                    Message = "Testificate"
                }
            ]
        });

        return Task.CompletedTask;
    }
}
