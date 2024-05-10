using QuantumCore.API.Game;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Commands
{
    [Command("logout", "Logout from the game")]
    [CommandNoPermission]
    public class LogoutCommand : ICommandHandler
    {
        private readonly IWorld _world;

        public LogoutCommand(IWorld world)
        {
            _world = world;
        }

        public Task ExecuteAsync(CommandContext context)
        {
            context.Player.SendChatInfo("Logging out. Please wait.");
            _world.DespawnPlayerAsync(context.Player);
            context.Player.Disconnect();
            return Task.CompletedTask;
        }
    }
}
