using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Caching;

namespace QuantumCore.Game.Commands
{
    [Command("logout", "Logout from the game")]
    [CommandNoPermission]
    public class LogoutCommand : ICommandHandler
    {
        private readonly IWorld _world;

        private readonly ICacheManager _cacheManager;

        public LogoutCommand(IWorld world, ICacheManager cacheManager)
        {
            _world = world;
            _cacheManager = cacheManager;
        }

        public async Task ExecuteAsync(CommandContext context)
        {
            context.Player.SendChatInfo("Logging out. Please wait.");
            await context.Player.CalculatePlayedTimeAsync();
            await _world.DespawnPlayerAsync(context.Player);
            await _cacheManager.Del("account:token:" + context.Player.Player.AccountId);
            context.Player.Disconnect();
        }
    }
}