using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace QuantumCore.Game.Persistence;

internal class MySqlGameDbContext : GameDbContext
{
    private readonly IOptionsSnapshot<DatabaseOptions> _options;
    private readonly ILoggerFactory _loggerFactory;

    public MySqlGameDbContext(IOptionsSnapshot<DatabaseOptions> options, ILoggerFactory loggerFactory)
    {
        _options = options;
        _loggerFactory = loggerFactory;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var opts = _options.Get(HostingOptions.MODE_GAME);
        optionsBuilder.UseLoggerFactory(_loggerFactory);
        optionsBuilder.UseMySql(opts.ConnectionString, ServerVersion.AutoDetect(opts.ConnectionString), mysql =>
        {
            // because MySQL does not support schemas: https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/issues/1100
            mysql.SchemaBehavior(MySqlSchemaBehavior.Translate, (schema, table) => $"{schema}.{table}");
        });
    }
}
