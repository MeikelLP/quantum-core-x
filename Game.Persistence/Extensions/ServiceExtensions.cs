﻿using Core.Persistence.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace QuantumCore.Game.Persistence.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddGameDatabase(this IServiceCollection services)
    {
        services.AddQuantumCoreDatabase();
        services.AddSingleton<IDbPlayerRepository, DbPlayerRepository>();

        return services;
    }
}