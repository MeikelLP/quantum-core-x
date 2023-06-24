using Core.Persistence;

namespace QuantumCore.Game.Persistence;

[System.ComponentModel.DataAnnotations.Schema.Table("items")]
public class Item : BaseModel
{
    public Guid PlayerId { get; set; }
    public uint ItemId { get; set; }
    public byte Window { get; set; }
    public uint Position { get; set; }
    public byte Count { get; set; }
}