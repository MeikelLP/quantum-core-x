using Microsoft.EntityFrameworkCore;

namespace QuantumCore.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseMySql("server=localhost;user=root;password=test123;database=account");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("current_timestamp");
                entity.Property(e => e.UpdatedBy).HasDefaultValueSql("current_timestamp");
                entity.Property(e => e.LastLogin).HasDefaultValueSql("current_timestamp");
                entity.Property(e => e.Status).HasDefaultValueSql("0");
            });
        }
    }
}