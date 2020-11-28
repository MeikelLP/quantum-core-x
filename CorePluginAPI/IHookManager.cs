namespace QuantumCore.API
{
    /// <summary>
    /// Manage and maintain all hooks into the main core for plugins.
    /// </summary>
    public interface IHookManager
    {
        /// <summary>
        /// Register the hook
        /// </summary>
        /// <param name="hook">Instance of hook class</param>
        /// <typeparam name="T">Hook type</typeparam>
        public void RegisterHook<T>(T hook) where T : IHook;

        /// <summary>
        /// Unregister the given hook
        /// </summary>
        /// <param name="hook">Instance of the hook class which is already registered</param>
        /// <typeparam name="T">Hook type</typeparam>
        public void UnregisterHook<T>(T hook) where T : IHook;
    }
}