﻿using Microsoft.Extensions.DependencyInjection;
using QuantumCore.API.PluginTypes;
using QuantumCore.Auth.Persistence;
using QuantumCore.Extensions;

namespace QuantumCore.Auth.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddQuantumCoreDatabase();
        services.AddQuantumCoreCache();
        services.Scan(scan =>
        {
            scan.FromAssemblyOf<AuthServer>()
                .AddClasses(classes => classes.AssignableTo<IPacketHandler>())
                .AsImplementedInterfaces()
                .WithSingletonLifetime();
        });
        services.AddSingleton<IAccountManager, AccountManager>();

        return services;
    }
}