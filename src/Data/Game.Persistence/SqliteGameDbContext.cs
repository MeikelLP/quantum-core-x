using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace QuantumCore.Game.Persistence;

internal class SqliteGameDbContext : GameDbContext
{
    private readonly IOptionsSnapshot<DatabaseOptions> _options;

    public SqliteGameDbContext(IOptionsSnapshot<DatabaseOptions> options)
    {
        _options = options;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var opts = _options.Get(HostingOptions.MODE_GAME);
        optionsBuilder.UseSqlite(opts.ConnectionString);
    }
}
