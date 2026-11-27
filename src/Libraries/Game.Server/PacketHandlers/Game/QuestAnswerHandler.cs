using Microsoft.Extensions.Logging;
using QuantumCore.Packets.Quest;
using QuantumCore.PluginTypes;

namespace QuantumCore.Game.PacketHandlers.Game;

public class QuestAnswerHandler : IGamePacketHandler<QuestAnswer>
{
    private readonly ILogger<QuestAnswerHandler> _logger;

    public QuestAnswerHandler(ILogger<QuestAnswerHandler> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(GamePacketContext<QuestAnswer> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player is null)
        {
            ctx.Connection.Close();
            return Task.CompletedTask;
        }

        _logger.LogInformation("Quest answer: {Answer}", ctx.Packet.Answer);
        player.CurrentQuest?.Answer(ctx.Packet.Answer);

        return Task.CompletedTask;
    }
}
