using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Constants;
using QuantumCore.Game.World.Entities;
namespace QuantumCore.Game.Commands
{
	[Command("logout", "Logout from the game")]
    public static class LogoutCommand
    {
    	[CommandMethod]
        public static async void Logout(IPlayerEntity player)
        {
            player.SendChatInfo("Logging out. Please wait.");
        	 // todo migrate to plugin api style as soon as more is implemented
            if (!(player is PlayerEntity p))
            {
                return;
            }
        	p.Connection.Close();
        }
    }
}