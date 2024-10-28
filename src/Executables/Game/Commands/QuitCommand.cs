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
            context.Player.SendChatInfo("End the game. Please wait.");
            context.Player.SendChatCommand("quit");
            await context.Player.CalculatePlayedTimeAsync();
            await _world.DespawnPlayerAsync(context.Player);
            context.Player.Disconnect();
        }
    }
}
