using Microsoft.Extensions.DependencyInjection;

namespace QuantumCore.API;

public interface IServiceCollectionPlugin
{
    void ModifyServiceCollection(IServiceCollection services);
}