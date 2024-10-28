using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace QuantumCore.Game.Persistence;

internal class MySqlGameDbContext : GameDbContext
{
    private readonly IOptions<DatabaseOptions> _options;
    private readonly ILoggerFactory _loggerFactory;

    public MySqlGameDbContext(IOptions<DatabaseOptions> options, ILoggerFactory loggerFactory)
    {
        _options = options;
        _loggerFactory = loggerFactory;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var opts = _options.Value;
        optionsBuilder.UseLoggerFactory(_loggerFactory);
        optionsBuilder.UseMySql(opts.ConnectionString, ServerVersion.AutoDetect(opts.ConnectionString), mysql =>
        {
            // because MySQL does not support schemas: https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/issues/1100
            mysql.SchemaBehavior(MySqlSchemaBehavior.Translate, (schema, table) => $"{schema}.{table}");
        });
    }
}