using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QuantumCore.Game.Persistence.Entities;

[Table("players")]
public class Player
{
    public required Guid Id { get; init; }
    public required Guid AccountId { get; init; }
    public required byte Empire { get; init; }
    public required byte PlayerClass { get; init; }
    public required byte SkillGroup { get; init; }
    [DefaultValue(0)] public required uint PlayTime { get; init; }
    [DefaultValue(1)] public required byte Level { get; init; }
    [DefaultValue(0)] public required uint Experience { get; init; }
    [DefaultValue(0)] public required uint Gold { get; init; }
    [DefaultValue(0)] public required byte St { get; init; }
    [DefaultValue(0)] public required byte Ht { get; init; }
    [DefaultValue(0)] public required byte Dx { get; init; }
    [DefaultValue(0)] public required byte Iq { get; init; }
    public required int PositionX { get; init; }
    public required int PositionY { get; init; }
    public required long Health { get; init; }
    public required long Mana { get; init; }
    public required long Stamina { get; init; }
    [DefaultValue(0)] public required uint BodyPart { get; init; }
    [DefaultValue(0)] public required uint HairPart { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
    [StringLength(24)] public required string Name { get; init; }
    [DefaultValue(0)] public required uint GivenStatusPoints { get; init; }
    [DefaultValue(0)] public required uint AvailableStatusPoints { get; init; }

    public static void Configure(EntityTypeBuilder<Player> builder, DatabaseFacade database)
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