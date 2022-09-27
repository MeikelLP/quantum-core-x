using System.Threading.Tasks;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Commands
{
	[Command("phase_select", "Go back to character selection")]
    [CommandNoPermission]
    public static class PhaseSelectCommand
    {
    	[CommandMethod]
        public static async Task PhaseSelect(IWorld world, IPlayerEntity player)
        {
            await player.SendChatInfo("Going back to character selection. Please wait.");

            // todo implement wait
            
            // Despawn player
            await world.DespawnEntity(player);
            
            // Bring client back to select menu
            if (player.Connection is GameConnection gc)
            {
                await gc.SetPhase(EPhases.Select);
            }
        }
    }
}