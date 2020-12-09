using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World.Entities;
namespace QuantumCore.Game.Commands
{
	[Command("quit", "quit the game")]
    public static class QuitCommand
    {
    	[CommandMethod]
        public static async void Quit(IPlayerEntity player)
        {
            player.SendChatInfo("End the game. Please wait.");
        	player.SendChatCommand("quit");
        	// todo migrate to plugin api style as soon as more is implemented
            if (!(player is PlayerEntity p))
            {
                return;
            }
        	p.Connection.Close();
        }
    }
}