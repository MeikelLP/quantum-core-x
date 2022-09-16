using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands
{
    [Command("kick", "Kick a player from the Server")]
    public static class KickCommand
    {
        [CommandMethod]
        public static async Task Kick(IPlayerEntity player, IPlayerEntity target)
        {
            target.Disconnect();
        }
    }
}
