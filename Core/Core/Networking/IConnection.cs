namespace QuantumCore.Core.Networking
{
    public interface IConnection
    {
        public void Start();
        public void Close();
        public void Send(object packet);
    }
}