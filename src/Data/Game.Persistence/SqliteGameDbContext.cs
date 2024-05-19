using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace QuantumCore.Game.Persistence;

internal class SqliteGameDbContext : GameDbContext
{
    private readonly IOptions<DatabaseOptions> _options;

    public SqliteGameDbContext(IOptions<DatabaseOptions> options)
    {
        _options = options;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var opts = _options.Value;
        optionsBuilder.UseSqlite(opts.ConnectionString);
    }
}