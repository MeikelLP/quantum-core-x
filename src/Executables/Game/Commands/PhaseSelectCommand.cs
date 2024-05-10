using Microsoft.Extensions.DependencyInjection;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Caching;
using QuantumCore.Extensions;
using QuantumCore.Game.Persistence;

namespace QuantumCore.Game.Commands
{
    [Command("phase_select", "Go back to character selection")]
    [CommandNoPermission]
    public class PhaseSelectCommand : ICommandHandler
    {
        private readonly IWorld _world;
        private readonly IServiceProvider _provider;
        private readonly ICacheManager _cache;

        public PhaseSelectCommand(IWorld world, IServiceProvider provider, ICacheManager cache)
        {
            _world = world;
            _provider = provider;
            _cache = cache;
        }

        public async Task ExecuteAsync(CommandContext context)
        {
            context.Player.SendChatInfo("Going back to character selection. Please wait.");

            // todo implement wait
            
            // Calculate session time
            var key = $"player:{context.Player.Player.Id}:loggedInTime";
            var startSessionTime = await _cache.Get<long>(key);
            
            var sessionTimeMillis = context.Player.Connection.Server.ServerTime - startSessionTime;
            var minutes = sessionTimeMillis / 60000; // milliseconds to minutes
            
            context.Player.AddPoint(EPoints.PlayTime, (int) minutes);
            
            //var manager = ActivatorUtilities.CreateInstance<DbPlayerRepository>(_provider);
            //await manager.SaveAsync(context.Player.Player);

            // Despawn player
            _world.DespawnEntity(context.Player);
            context.Player.Connection.SetPhase(EPhases.Select);
        }
    }
}
