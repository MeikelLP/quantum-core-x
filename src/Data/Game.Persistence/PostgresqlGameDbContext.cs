using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace QuantumCore.Game.Persistence;

internal class PostgresqlGameDbContext : GameDbContext
{
    private readonly IOptions<DatabaseOptions> _options;

    public PostgresqlGameDbContext(IOptions<DatabaseOptions> options)
    {
        _options = options;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var opts = _options.Value;
        options.UseNpgsql(opts.ConnectionString);
    }
}