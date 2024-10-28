using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace QuantumCore.Auth.Persistence;

internal class MySqlAuthDbContext : AuthDbContext
{
    private readonly IOptionsSnapshot<DatabaseOptions> _options;

    public MySqlAuthDbContext(IOptionsSnapshot<DatabaseOptions> options, ILoggerFactory loggerFactory) : base(
        loggerFactory)
    {
        _options = options;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        var opts = _options.Get("auth");
        optionsBuilder.UseMySql(opts.ConnectionString, ServerVersion.AutoDetect(opts.ConnectionString), mysql =>
        {
            // because MySQL does not support schemas: https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/issues/1100
            mysql.SchemaBehavior(MySqlSchemaBehavior.Translate, (schema, table) => $"{schema}.{table}");
        });
    }
}