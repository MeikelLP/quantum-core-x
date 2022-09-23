using System.Threading;
using System.Threading.Tasks;

namespace QuantumCore.API;

public interface IConnectionLifetimeListener
{
    Task OnConnectedAsync(CancellationToken token);
    Task OnDisconnectedAsync(CancellationToken token);
}