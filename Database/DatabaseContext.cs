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
    }
}