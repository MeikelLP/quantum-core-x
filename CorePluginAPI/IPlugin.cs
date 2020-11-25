namespace QuantumCore.API
{
    public interface IPlugin
    {
        public string Name { get; }
        public string Author { get; }

        public void Register(object server);
        public void Unregister();
    }
}