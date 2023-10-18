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

        public async Task ExecuteAsync(CommandContext context)
        {
            context.Player.SendChatInfo("Logging out. Please wait.");
            _world.DespawnEntity(context.Player);
            context.Player.Disconnect();
        }
    }
}
