using Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuantumCore.Game.Persistence;

internal class PostgresqlGameDbContext : GameDbContext
{
    private readonly IOptionsSnapshot<DatabaseOptions> _options;

    public PostgresqlGameDbContext(IOptionsSnapshot<DatabaseOptions> options, ILoggerFactory loggerFactory,
        IHostEnvironment hostEnvironment) :
        base(loggerFactory, hostEnvironment)
    {
        _options = options;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var opts = _options.Get(HostingOptions.MODE_GAME);
        options.UseNpgsql(opts.ConnectionString);
    }
}