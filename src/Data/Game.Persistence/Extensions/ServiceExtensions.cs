using Core.Persistence.Extensions;
using Microsoft.Extensions.DependencyInjection;
using QuantumCore.API.Data;

namespace QuantumCore.Game.Persistence.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddGameDatabase(this IServiceCollection services)
    {
        services.AddQuantumCoreDatabase();
        services.AddSingleton<IDbPlayerRepository, DbPlayerRepository>();
        services.AddSingleton<ICommandPermissionRepository, CommandPermissionRepository>();
        services.AddSingleton<IEmpireRepository, EmpireRepository>();
        services.AddSingleton<IItemRepository, ItemRepository>();

        return services;
    }
}
