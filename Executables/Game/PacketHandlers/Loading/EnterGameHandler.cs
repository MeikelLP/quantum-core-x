using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Extensions;
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
        
        public async Task ExecuteAsync(GamePacketContext<EnterGame> ctx, CancellationToken token = default)
        {
            var player = ctx.Connection.Player;
            if (player == null)
            {
                _logger.LogWarning("Trying to enter game without a player!");
                ctx.Connection.Close();
                return;
            }
            
            // Enable game phase
            await ctx.Connection.SetPhaseAsync(EPhases.Game);
            
            await ctx.Connection.Send(new GameTime { Time = (uint) ctx.Connection.Server.ServerTime });
            await ctx.Connection.Send(new Channel { ChannelNo = 1 }); // todo
            
            // Show the player
            await player.Show(ctx.Connection);
            
            // Spawn the player
            if (!await _world.SpawnEntity(player))
            {
                _logger.LogWarning("Failed to spawn player entity");
                ctx.Connection.Close();
            }
            
            await player.SendInventory();
        }
    }
}