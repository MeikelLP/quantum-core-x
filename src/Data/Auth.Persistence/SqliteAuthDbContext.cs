using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuantumCore.Auth.Persistence;

internal class SqliteAuthDbContext : AuthDbContext
{
    private readonly IOptions<DatabaseOptions> _options;

    public SqliteAuthDbContext(IOptions<DatabaseOptions> options, ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        _options = options;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        var opts = _options.Value;
        optionsBuilder.UseSqlite(opts.ConnectionString);
    }
}