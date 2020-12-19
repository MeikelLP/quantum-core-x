using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Constants;
using QuantumCore.Game.World.Entities;
namespace QuantumCore.Game.Commands
{
	[Command("phase_select", "Go back to character selection")]
    public static class PhaseSelectCommand
    {
    	[CommandMethod]
        public static async void PhaseSelect(IPlayerEntity player)
        {
            player.SendChatInfo("Going back to character selection. Please wait.");

            // todo implement wait
            
            // Despawn player
            World.World.Instance.DespawnEntity(player);

            // todo migrate to core api
            if (player is PlayerEntity p)
            {
                // Bring client back to select menu
                p.Connection.SetPhase(EPhases.Select);
            }
        }
    }
}