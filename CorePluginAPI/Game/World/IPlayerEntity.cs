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
    }
}