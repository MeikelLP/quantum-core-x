using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QuantumCore.Auth.Persistence.Entities;

[Table("accounts")]
public class Account
{
    public Guid Id { get; init; }
    [StringLength(30)] public required string Username { get; init; } = "";
    [StringLength(60)] public required string Password { get; set; } = "";
    [StringLength(100)] public required string Email { get; init; } = "";
    [DefaultValue(1)] public required short Status { get; init; }
    public DateTime? LastLogin { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
    [StringLength(7)] public required string DeleteCode { get; init; } = "";

    public AccountStatus AccountStatus { get; init; } = null!;

    public static void Configure(EntityTypeBuilder<Account> builder, DatabaseFacade database)
    {
        builder
            .HasOne(x => x.AccountStatus)
            .WithMany(x => x.Accounts)
            .HasForeignKey(x => x.Status)
            .IsRequired();
        if (database.IsSqlite() || database.IsNpgsql())
        {
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("current_timestamp");
            builder.Property(x => x.UpdatedAt).HasDefaultValueSql("current_timestamp");
        }
        else if (database.IsMySql())
        {
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))");
            builder.Property(x => x.UpdatedAt).HasDefaultValueSql("(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))");
        }

        builder.HasData([
            new Account
            {
                Id = Guid.Parse("E34FD5AB-FB3B-428E-935B-7DB5BD08A3E5"),
                Username = "admin",
                Password = "$2y$10$5e9nP50E64iy8vaSMwrRWO7vCfnA7.p5XpIDHC3hPdi6BCtTF7rBS", // admin
                Email = "admin@test.com",
                Status = 1,
                CreatedAt = default,
                UpdatedAt = default,
                DeleteCode = "1234567"
            }
        ]);
    }
}