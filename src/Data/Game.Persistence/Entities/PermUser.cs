using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QuantumCore.Game.Persistence.Entities;

public class PermUser
{
    public required Guid GroupId { get; init; }
    public required uint PlayerId { get; init; }

    public PermGroup Group { get; set; } = null!;
    public Player Player { get; set; } = null!;

    public static void Configure(EntityTypeBuilder<PermUser> builder, DatabaseFacade database)
    {
        builder.HasKey(x => new {x.GroupId, x.PlayerId});
        builder.HasData([
            new PermUser
            {
                PlayerId = 1,
                GroupId = PermGroup.OperatorGroup
            }
        ]);
    }
}
