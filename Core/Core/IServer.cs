using System.Threading.Tasks;

namespace QuantumCore.Core
{
    internal interface IServer
    {
        Task Init();
        Task Start();
    }
}