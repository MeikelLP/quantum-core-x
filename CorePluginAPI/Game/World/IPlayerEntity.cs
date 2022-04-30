using QuantumCore.API.Game.Types;

namespace QuantumCore.API.Game.World
{
    public interface IPlayerEntity : IEntity
    {
        public string Name { get; }
        
        public IConnection Connection { get; }

        /// <summary>
        /// Sets the given point to the specified value
        /// </summary>
        /// <param name="point">Point to update</param>
        /// <param name="value">New value</param>
        public void SetPoint(EPoints point, uint value);
        
        /// <summary>
        /// Adds the value to the given point
        /// </summary>
        /// <param name="point">Point to update</param>
        /// <param name="value">Value to add (use negative value if you need to lower the point)</param>
        public void AddPoint(EPoints point, int value);
        
        /// <summary>
        /// Gets the value of the point
        /// </summary>
        /// <param name="point">Point to query</param>
        /// <returns>Current value of the point</returns>
        public uint GetPoint(EPoints point);
        
        /// <summary>
        /// Sends all points to the player
        /// </summary>
        public void SendPoints();

        /// <summary>
        /// Respawns the player if the player is dead.
        /// Does nothing if the player is alive
        /// </summary>
        /// <param name="town">If true the player will respawn in town instead of last location</param>
        public void Respawn(bool town);
        
        /// <summary>
        /// Sends the given message to the player
        /// </summary>
        /// <param name="message">Message to send</param>
        public void SendChatMessage(string message);
        
        /// <summary>
        /// Sends the given chat command to the player
        /// </summary>
        /// <param name="command">Command to send</param>
        public void SendChatCommand(string command);
        
        /// <summary>
        /// Sends the given chat info to the player
        /// </summary>
        /// <param name="info">Info message to send</param>
        public void SendChatInfo(string info);

        /// <summary>
        /// Refresh the target information on the client.
        /// </summary>
        public void SendTarget();

        public void Disconnect();
    }
}