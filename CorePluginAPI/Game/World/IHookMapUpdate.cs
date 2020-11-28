namespace QuantumCore.API.Game.World
{
    public interface IHookMapUpdate : IHook
    {
        /// <summary>
        /// Calls everytime a map gets updated by the server
        /// </summary>
        /// <param name="map">The map which is getting updated</param>
        /// <param name="elapsedTime">Time in milliseconds since the last update</param>
        public void HookMapUpdate(IMap map, double elapsedTime);
    }
}