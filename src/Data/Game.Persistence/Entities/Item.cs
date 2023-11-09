using System.ComponentModel.DataAnnotations.Schema;
using Core.Persistence;

namespace QuantumCore.Game.Persistence.Entities;

[Table("items")]
internal class Item : BaseModel
{
    public Guid PlayerId { get; set; }
    public uint ItemId { get; set; }
    public byte Window { get; set; }
    public uint Position { get; set; }
    public byte Count { get; set; }
}