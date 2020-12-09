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
         	// todo migrate to plugin api style as soon as more is implemented
            if (!(player is PlayerEntity p))
            {
                return;
            }
        	p.NearbyEntities.Remove(p);
        	p.Connection.SetPhase(EPhases.Select);
        }
    }
}