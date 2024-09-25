﻿using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QuantumCore.Game.Persistence.Entities.Guilds;

public class GuildMember
{
    public uint GuildId { get; set; }
    public uint PlayerId { get; set; }
    public byte RankId { get; set; }
    public bool IsLeader { get; set; }
    public uint SpentExperience { get; set; }

    public Guild Guild { get; set; } = null!;
    public Player Player { get; set; } = null!;
    public GuildRank Rank { get; set; } = null!;

    public static void Configure(EntityTypeBuilder<GuildMember> builder, DatabaseFacade database)
    {
        builder.HasKey(x => new {x.GuildId, x.PlayerId});
        builder.HasIndex(x => new {x.PlayerId}).IsUnique();
        builder.HasOne(x => x.Rank)
            .WithMany(x => x.Members)
            .HasForeignKey(x => new {x.GuildId, x.RankId})
            .HasPrincipalKey(x => new {x.GuildId, RankId = x.Rank});

        builder.HasOne(x => x.Player)
            .WithMany(x => x.Guilds);

        builder.HasOne(x => x.Guild)
            .WithMany(x => x.Members);
    }
}