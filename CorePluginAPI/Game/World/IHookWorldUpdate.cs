namespace QuantumCore.API.Game.World
{
    public interface IHookWorldUpdate : IHook
    {
        /// <summary>
        /// Called on every world update
        /// </summary>
        /// <param name="elapsedTime">Elapsed time in milliseconds since the last update</param>
        public void HookWorldUpdate(double elapsedTime);
    }
}