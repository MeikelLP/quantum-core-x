using System.Threading;
using System.Threading.Tasks;

namespace QuantumCore.API.PluginTypes;

public interface ISingletonPlugin
{
    Task InitializeAsync(CancellationToken token = default);
}