using System.Threading.Tasks;
using QuantumCore.Core.Constants;
using QuantumCore.Core.Networking;
using QuantumCore.Game.Packets;
using Serilog;

namespace QuantumCore.Game
{
    public static class PhaseLoading
    {
        [Listener(typeof(EnterGame))]
        public static async Task OnEnterGame(this GameConnection connection, EnterGame packet)
        {
            var player = connection.Player;
            if (player == null)
            {
                Log.Warning($"Trying to enter game without a player!");
                connection.Close();
                return;
            }
            
            // Enable game phase
            await connection.SetPhase(EPhases.Game);
            
            await connection.Send(new GameTime { Time = (uint) connection.Server.ServerTime });
            await connection.Send(new Channel { ChannelNo = 1 }); // todo
            
            // Show the player
            player.Show(connection);
            
            // Spawn the player
            if (!World.World.Instance.SpawnEntity(player))
            {
                Log.Warning("Failed to spawn player entity");
                connection.Close();
            }
            
            player.SendInventory();
        }
    }
}