namespace QuantumCore.API.Game.World
{
    public interface IPlayerEntity : IEntity
    {
        public string Name { get; }

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