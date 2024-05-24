using Microsoft.Extensions.Logging;

namespace QuantumCore;

public interface IPluginExecutor
{
    Task ExecutePlugins<T>(ILogger logger, Func<T, Task> action);
}