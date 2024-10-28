using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace QuantumCore.Game.Persistence;

internal class PostgresqlGameDbContext : GameDbContext
{
    private readonly IOptionsSnapshot<DatabaseOptions> _options;

    public PostgresqlGameDbContext(IOptionsSnapshot<DatabaseOptions> options)
    {
        _options = options;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var opts = _options.Get("game");
        options.UseNpgsql(opts.ConnectionString);
    }
}