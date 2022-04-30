using QuantumCore.API.Game;
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
        public static async void PhaseSelect(IPlayerEntity player)
        {
            player.SendChatInfo("Going back to character selection. Please wait.");

            // todo implement wait
            
            // Despawn player
            World.World.Instance.DespawnEntity(player);
            
            // Bring client back to select menu
            (player.Connection as GameConnection)?.SetPhase(EPhases.Select);
        }
    }
}