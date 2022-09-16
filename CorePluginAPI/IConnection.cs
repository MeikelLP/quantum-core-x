using System.Threading.Tasks;

namespace QuantumCore.API
{
    public interface IConnection
    {
        public Task Start();
        public void Close();
        public void Send(object packet);
    }
}