using QuantumCore.Core.Constants;
using QuantumCore.Game.Packets;
using Serilog;

namespace QuantumCore.Game
{
    public static class PhaseLoading
    {
        public static async void OnEnterGame(this GameConnection connection, EnterGame packet)
        {
            var player = connection.Player;
            if (player == null)
            {
                Log.Warning($"Trying to enter game without a player!");
                connection.Close();
                return;
            }
            
            // Enable game phase
            connection.SetPhase(EPhases.Game);
            
            connection.Send(new GameTime { Time = (uint) connection.Server.ServerTime });
            connection.Send(new Channel { ChannelNo = 1 }); // todo
            
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