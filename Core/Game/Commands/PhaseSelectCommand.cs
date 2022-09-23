using System.Threading.Tasks;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Constants;
using QuantumCore.Game.World.Entities;
namespace QuantumCore.Game.Commands
{
	[Command("phase_select", "Go back to character selection")]
    [CommandNoPermission]
    public static class PhaseSelectCommand
    {
    	[CommandMethod]
        public static async Task PhaseSelect(IPlayerEntity player)
        {
            await player.SendChatInfo("Going back to character selection. Please wait.");

            // todo implement wait
            
            // Despawn player
            await World.World.Instance.DespawnEntity(player);
            
            // Bring client back to select menu
            if (player.Connection is GameConnection gc)
            {
                await gc.SetPhase(EPhases.Select);
            }
        }
    }
}