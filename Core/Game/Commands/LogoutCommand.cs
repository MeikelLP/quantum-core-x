using System.Threading.Tasks;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Constants;
using QuantumCore.Game.World.Entities;
namespace QuantumCore.Game.Commands
{
	[Command("logout", "Logout from the game")]
    [CommandNoPermission]
    public static class LogoutCommand
    {
    	[CommandMethod]
        public static async Task Logout(IPlayerEntity player)
        {
            player.SendChatInfo("Logging out. Please wait.");
            player.Disconnect();
        }
    }
}