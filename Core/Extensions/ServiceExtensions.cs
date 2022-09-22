using Microsoft.Extensions.DependencyInjection;
using QuantumCore.Core.Networking;

namespace QuantumCore.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IPacketManager, DefaultPacketManager>();
        return services;
    }
}