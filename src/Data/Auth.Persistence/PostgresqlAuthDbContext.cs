using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuantumCore.Auth.Persistence;

internal class PostgresqlAuthDbContext : AuthDbContext
{
    private readonly IOptionsSnapshot<DatabaseOptions> _options;

    public PostgresqlAuthDbContext(IOptionsSnapshot<DatabaseOptions> options, ILoggerFactory loggerFactory) : base(
        loggerFactory)
    {
        _options = options;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        var opts = _options.Get(HostingOptions.MODE_AUTH);
        optionsBuilder.UseNpgsql(opts.ConnectionString);
    }
}
