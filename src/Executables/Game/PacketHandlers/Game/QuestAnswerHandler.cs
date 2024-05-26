using Microsoft.Extensions.Logging;
using QuantumCore.Game.Packets.Quest;

namespace QuantumCore.Game.PacketHandlers.Game;

[PacketHandler(typeof(QuestAnswer))]
public class QuestAnswerHandler
{
    private readonly ILogger<QuestAnswerHandler> _logger;

    public QuestAnswerHandler(ILogger<QuestAnswerHandler> logger)
    {
        _logger = logger;
    }

    public void Execute(GamePacketContext ctx, QuestAnswer packet)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return;
        }

        _logger.LogInformation("Quest answer: {Answer}", packet.Answer);
        player.CurrentQuest?.Answer(packet.Answer);
    }
}