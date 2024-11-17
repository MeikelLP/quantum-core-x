﻿using Core.Persistence.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace QuantumCore.Game.Persistence.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddGameDatabase(this IServiceCollection services)
    {
        services.AddQuantumCoreDatabase(HostingOptions.ModeGame);
        services.AddDbContext<MySqlGameDbContext>();
        services.AddDbContext<PostgresqlGameDbContext>();
        services.AddDbContext<SqliteGameDbContext>();
        services.AddScoped<GameDbContext>(provider =>
        {
            var options = provider.GetRequiredService<IOptionsSnapshot<DatabaseOptions>>().Get(HostingOptions.ModeGame);
            return options.Provider switch
            {
                DatabaseProvider.Mysql => provider.GetRequiredService<MySqlGameDbContext>(),
                DatabaseProvider.Postgresql => provider.GetRequiredService<PostgresqlGameDbContext>(),
                DatabaseProvider.Sqlite => provider.GetRequiredService<SqliteGameDbContext>(),
                _ => throw new InvalidOperationException(
                    $"Cannot create db context for out of range provider: {options.Provider}")
            };
        });
        services.AddScoped<IDbPlayerRepository, DbPlayerRepository>();
        services.AddScoped<IDbPlayerSkillsRepository, DbPlayerSkillsRepository>();
        services.AddScoped<ICommandPermissionRepository, CommandPermissionRepository>();
        services.AddScoped<IItemRepository, ItemRepository>();

        return services;
    }
}