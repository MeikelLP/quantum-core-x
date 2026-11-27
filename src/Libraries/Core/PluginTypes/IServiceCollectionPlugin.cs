using Microsoft.Extensions.DependencyInjection;

namespace QuantumCore.PluginTypes;

public interface IServiceCollectionPlugin
{
    void ModifyServiceCollection(IServiceCollection services);
}