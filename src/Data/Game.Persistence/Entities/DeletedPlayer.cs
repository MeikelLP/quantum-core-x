using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QuantumCore.Game.Persistence.Entities;

public class DeletedPlayer
{
    public required Guid Id { get; init; }
    public required Guid AccountId { get; init; }
    public required byte PlayerClass { get; init; }
    public required byte SkillGroup { get; init; }
    [DefaultValue(0)] public required int PlayTime { get; init; }
    [DefaultValue(1)] public required byte Level { get; init; }
    [DefaultValue(0)] public required int Experience { get; init; }
    [DefaultValue(0)] public required byte Gold { get; init; }
    [DefaultValue(0)] public required byte St { get; init; }
    [DefaultValue(0)] public required byte Ht { get; init; }
    [DefaultValue(0)] public required byte Dx { get; init; }
    [DefaultValue(0)] public required byte Iq { get; init; }
    public required int PositionX { get; init; }
    public required int PositionY { get; init; }
    public required long Health { get; init; }
    public required long Mana { get; init; }
    public required long Stamina { get; init; }
    [DefaultValue(0)] public required int BodyPart { get; init; }
    [DefaultValue(0)] public required int HairPart { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public required DateTime DeletedAt { get; init; }
    [StringLength(24)] public required string Name { get; init; }

    public static void Configure(EntityTypeBuilder<DeletedPlayer> builder, DatabaseFacade database)
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
    }
}
