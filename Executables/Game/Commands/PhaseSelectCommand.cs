using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Extensions;

namespace QuantumCore.Game.Commands
{
    [Command("phase_select", "Go back to character selection")]
    [CommandNoPermission]
    public class PhaseSelectCommand : ICommandHandler
    {
        private readonly IWorld _world;

        public PhaseSelectCommand(IWorld world)
        {
            _world = world;
        }

        public async Task ExecuteAsync(CommandContext context)
        {
            context.Player.SendChatInfo("Going back to character selection. Please wait.");

            // todo implement wait

            // Despawn player
            _world.DespawnEntity(context.Player);
            context.Player.Connection.SetPhaseAsync(EPhases.Select);
        }
    }
}
