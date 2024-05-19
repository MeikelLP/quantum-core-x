using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QuantumCore.Game.Persistence.Entities.Guilds;

public class Guild
{
    public uint Id { get; set; }

    [StringLength(12)] public required string Name { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public uint OwnerId { get; set; }
    public byte Level { get; set; }
    public uint Experience { get; set; }
    public ushort MaxMemberCount { get; set; }
    public uint Gold { get; set; }

    public ICollection<GuildMember> Members { get; set; } = null!;
    public ICollection<GuildRank> Ranks { get; set; } = null!;
    public Player Leader { get; set; } = null!;

    public static void Configure(EntityTypeBuilder<Guild> builder, DatabaseFacade database)
    {
        builder.HasOne(x => x.Leader)
            .WithMany(x => x.GuildsToLead)
            .HasForeignKey(x => x.OwnerId);

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