using QuantumCore.Auth.Persistence.Extensions;
using QuantumCore.Caching.Extensions;
using QuantumCore.Networking;

namespace QuantumCore.Auth.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddAuthDatabase();
        services.AddQuantumCoreCaching();
        services.AddSingleton<IPacketReader2, PacketReader2<>>();

        return services;
    }
}