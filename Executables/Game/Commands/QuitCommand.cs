using QuantumCore.API.Game;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Commands
{
    [Command("quit", "Quit the game")]
    [CommandNoPermission]
    public class QuitCommand : ICommandHandler
    {
        private readonly IWorld _world;

        public QuitCommand(IWorld world)
        {
            _world = world;
        }
        
        public async Task ExecuteAsync(CommandContext context)
        {
            await context.Player.SendChatInfo("End the game. Please wait.");
            await context.Player.SendChatCommand("quit");
            await _world.DespawnEntity(context.Player);
            context.Player.Disconnect();
        }
    }
}