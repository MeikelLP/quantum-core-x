using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuantumCore.Auth.Persistence.Entities;

namespace QuantumCore.Auth.Persistence;

public abstract class AuthDbContext : DbContext
{
    private readonly ILoggerFactory _loggerFactory;
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<AccountStatus> AccountStatus { get; set; } = null!;

    public AuthDbContext(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLoggerFactory(_loggerFactory);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        Account.Configure(modelBuilder.Entity<Account>(), Database);
        Auth.Persistence.Entities.AccountStatus.Configure(modelBuilder.Entity<AccountStatus>(), Database);
    }
}