using System.Threading;
using System.Threading.Tasks;

namespace QuantumCore.API;

public interface ISingletonPlugin
{
    Task InitializeAsync(CancellationToken token = default);
}