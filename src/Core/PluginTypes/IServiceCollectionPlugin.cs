using Microsoft.Extensions.DependencyInjection;

namespace QuantumCore.API.PluginTypes;

public interface IServiceCollectionPlugin
{
    void ModifyServiceCollection(IServiceCollection services);
}