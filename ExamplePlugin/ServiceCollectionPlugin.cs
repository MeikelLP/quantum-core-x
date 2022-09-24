using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QuantumCore.API;

namespace ExamplePlugin;

public class ServiceCollectionPlugin : IServiceCollectionPlugin
{
    public void ModifyServiceCollection(IServiceCollection services)
    {
        services.Replace(new ServiceDescriptor(typeof(IQuestManager), typeof(CustomQuestManager)));
    }
}