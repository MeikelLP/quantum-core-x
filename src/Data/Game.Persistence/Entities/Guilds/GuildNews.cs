using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuantumCore.API.Game.Guild;

namespace QuantumCore.Game.Persistence.Entities.Guilds;

public class GuildNews
{
    public uint Id { get; set; }
    public uint PlayerId { get; set; }

    [StringLength(GuildConstants.NewsMessageMaxLength)]
    public string Message { get; set; } = "";

    public Player Player { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public uint? GuildId { get; set; }
    public Guild Guild { get; set; } = null!;

    public static void Configure(EntityTypeBuilder<GuildNews> builder, DatabaseFacade database)
    {
        builder.HasOne(x => x.Player)
            .WithMany(x => x.WrittenGuildNews)
            .HasForeignKey(x => x.PlayerId);
        builder.HasOne(x => x.Guild)
            .WithMany(x => x.News)
            .HasForeignKey(x => x.GuildId);

        if (database.IsSqlite() || database.IsNpgsql())
        {
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("current_timestamp");
        }
        else if (database.IsMySql())
        {
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))");
        }
    }
}
