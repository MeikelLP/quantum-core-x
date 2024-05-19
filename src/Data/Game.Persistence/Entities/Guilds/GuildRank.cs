using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuantumCore.API.Game.Guild;

namespace QuantumCore.Game.Persistence.Entities.Guilds;

public class GuildRank
{
    public uint GuildId { get; set; }
    public byte Rank { get; set; }
    [StringLength(8)] public string Name { get; set; } = "";
    public GuildRankPermission Permissions { get; set; }

    public Guild Guild { get; set; } = null!;
    public ICollection<GuildMember> Members { get; set; } = null!;

    public static void Configure(EntityTypeBuilder<GuildRank> builder, DatabaseFacade database)
    {
        builder.HasKey(x => new {x.GuildId, RankId = x.Rank});
        builder.HasOne(x => x.Guild).WithMany(x => x.Ranks);
        builder.HasMany(x => x.Members).WithOne(x => x.Rank);
    }
}
