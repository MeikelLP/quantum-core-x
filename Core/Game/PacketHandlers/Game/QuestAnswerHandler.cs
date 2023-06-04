using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets.Quest;

namespace QuantumCore.Game.PacketHandlers.Game
{
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
            if (player == null)
            {
                ctx.Connection.Close();
                return Task.CompletedTask;
            }
            
            _logger.LogInformation("Quest answer: {Answer}", ctx.Packet.Answer);
            player.CurrentQuest?.Answer(ctx.Packet.Answer);

            return Task.CompletedTask;
        }
    }
}