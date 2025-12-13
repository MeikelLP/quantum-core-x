using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.API.PluginTypes;
using QuantumCore.Caching;
using QuantumCore.Extensions;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Loading
{
    public class EnterGameHandler : IGamePacketHandler<EnterGame>
    {
        private readonly ILogger<EnterGameHandler> _logger;
        private readonly IWorld _world;
        private readonly ICacheManager _cache;

        public EnterGameHandler(ILogger<EnterGameHandler> logger, IWorld world, ICacheManager cache)
        {
            _logger = logger;
            _world = world;
            _cache = cache;
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
            ctx.Connection.SetPhase(EPhase.Game);

            ctx.Connection.Send(new GameTime {Time = (uint)ctx.Connection.Server.ServerTime});
            ctx.Connection.Send(new Channel {ChannelNo = 1}); // todo

            var key = $"player:{player.Player.Id}:loggedInTime";
            await _cache.Server.Set(key, ctx.Connection.Server.ServerTime);

            player.ShowEntity(ctx.Connection);
            _world.SpawnEntity(player);

            player.SendInventory();
            player.Skills.Send();
        }
    }
}
