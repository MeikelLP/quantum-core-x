using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QuantumCore.Game.Persistence.Entities;

public class PlayerQuickSlot
{
    public uint PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    public byte Slot { get; set; }
    public byte Type { get; set; }

    /// <summary>
    /// Skill ID or Item ID depending on the type
    /// </summary>
    public byte Value { get; set; }


    public static void Configure(EntityTypeBuilder<PlayerQuickSlot> builder, DatabaseFacade database)
    {
        builder.HasKey(x => new {x.PlayerId, x.Slot});
    }
}
