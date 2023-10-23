using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.API.PluginTypes;
using QuantumCore.Extensions;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Loading
{
    public class EnterGameHandler : IGamePacketHandler<EnterGame>
    {
        private readonly ILogger<EnterGameHandler> _logger;
        private readonly IWorld _world;

        public EnterGameHandler(ILogger<EnterGameHandler> logger, IWorld world)
        {
            _logger = logger;
            _world = world;
        }

        public Task ExecuteAsync(GamePacketContext<EnterGame> ctx, CancellationToken token = default)
        {
            var player = ctx.Connection.Player;
            if (player == null)
            {
                _logger.LogWarning("Trying to enter game without a player!");
                ctx.Connection.Close();
                return Task.CompletedTask;
            }

            // Enable game phase
            ctx.Connection.SetPhase(EPhases.Game);

            ctx.Connection.Send(new GameTime { Time = (uint) ctx.Connection.Server.ServerTime });
            ctx.Connection.Send(new Channel { ChannelNo = 1 }); // todo

            player.ShowEntity(ctx.Connection);
            _world.SpawnEntity(player);

            player.SendInventory();
            player.SendCharacterUpdate();
            return Task.CompletedTask;
        }
    }
}
