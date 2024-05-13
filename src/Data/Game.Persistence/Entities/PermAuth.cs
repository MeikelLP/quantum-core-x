using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QuantumCore.Game.Persistence.Entities;

public class PermAuth
{
    public required Guid Id { get; init; }
    public required Guid GroupId { get; init; }

    [StringLength(255)] public required string Command { get; init; }

    public PermGroup Group { get; set; } = null!;

    public static void Configure(EntityTypeBuilder<PermAuth> builder, DatabaseFacade database)
    {
        if (database.IsNpgsql())
        {
            builder.Property(x => x.Id).ValueGeneratedOnAdd().HasDefaultValueSql("gen_random_uuid()");
        }
    }
}
