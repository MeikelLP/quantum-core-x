using Microsoft.EntityFrameworkCore;
using QuantumCore.Game.Persistence.Entities;

namespace QuantumCore.Game.Persistence;

public abstract class GameDbContext : DbContext
{
    public DbSet<Player> Players { get; set; } = null!;
    public DbSet<PlayerSkill> PlayerSkills { get; set; } = null!;
    public DbSet<DeletedPlayer> DeletedPlayers { get; set; } = null!;
    public DbSet<Item> Items { get; set; } = null!;
    public DbSet<PermAuth> Permissions { get; set; } = null!;
    public DbSet<PermGroup> PermissionGroups { get; set; } = null!;
    public DbSet<PermUser> PermissionUsers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        DeletedPlayer.Configure(modelBuilder.Entity<DeletedPlayer>(), Database);
        Player.Configure(modelBuilder.Entity<Player>(), Database);
        PlayerSkill.Configure(modelBuilder.Entity<PlayerSkill>(), Database);
        Item.Configure(modelBuilder.Entity<Item>(), Database);
        PermAuth.Configure(modelBuilder.Entity<PermAuth>(), Database);
        PermGroup.Configure(modelBuilder.Entity<PermGroup>(), Database);
        PermUser.Configure(modelBuilder.Entity<PermUser>(), Database);
    }
}
