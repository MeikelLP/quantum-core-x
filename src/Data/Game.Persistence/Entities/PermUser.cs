using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QuantumCore.Game.Persistence.Entities;

[Table("perm_users")]
public class PermUser
{
    public required Guid GroupId { get; init; }
    public required Guid PlayerId { get; init; }

    public PermGroup Group { get; set; } = null!;
    public Player Player { get; set; } = null!;

    public static void Configure(EntityTypeBuilder<PermUser> builder, DatabaseFacade database)
    {
        builder.HasKey(x => new {x.GroupId, x.PlayerId});
    }
}