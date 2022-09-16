using System.Threading.Tasks;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World.Entities;
namespace QuantumCore.Game.Commands
{
	[Command("quit", "Quit the game")]
    [CommandNoPermission]
    public static class QuitCommand
    {
    	[CommandMethod]
        public static async Task Quit(IPlayerEntity player)
        {
            player.SendChatInfo("End the game. Please wait.");
        	player.SendChatCommand("quit");
            player.Disconnect();
        }
    }
}