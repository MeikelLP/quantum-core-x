using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuantumCore.Auth.Persistence.Entities;

namespace QuantumCore.Auth.Persistence;

public abstract class AuthDbContext : DbContext
{
    private readonly ILoggerFactory _loggerFactory;
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<AccountStatus> AccountStatus { get; set; } = null!;

    protected AuthDbContext(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);
        optionsBuilder.UseLoggerFactory(_loggerFactory);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        Account.Configure(modelBuilder.Entity<Account>(), Database);
        Entities.AccountStatus.Configure(modelBuilder.Entity<AccountStatus>(), Database);
    }
}