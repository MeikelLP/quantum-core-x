using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QuantumCore.Game.Persistence.Entities;

public class Item
{
    public required Guid Id { get; init; }
    public required uint PlayerId { get; init; }
    public required uint ItemId { get; init; }
    public required byte Window { get; init; }
    public required uint Position { get; init; }
    public required byte Count { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }

    public Player Player { get; set; } = null!;

    public static void Configure(EntityTypeBuilder<Item> builder, DatabaseFacade database)
    {
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

        if (database.IsNpgsql())
        {
            builder.Property(x => x.Id).ValueGeneratedOnAdd().HasDefaultValueSql("gen_random_uuid()");
        }
    }
}
