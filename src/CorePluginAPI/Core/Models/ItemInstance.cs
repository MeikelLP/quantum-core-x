namespace QuantumCore.API.Core.Models;

public class ItemInstance
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public uint ItemId { get; set; }
    public byte Window { get; set; }
    public uint Position { get; set; }
    public byte Count { get; set; }
}