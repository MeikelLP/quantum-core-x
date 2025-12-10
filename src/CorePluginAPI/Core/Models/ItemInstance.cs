using QuantumCore.API.Game.Types.Items;

namespace QuantumCore.API.Core.Models;

public class ItemInstance
{
    public Guid Id { get; set; }
    public uint PlayerId { get; set; }
    public uint ItemId { get; set; }
    public WindowType Window { get; set; }
    public uint Position { get; set; }
    public byte Count { get; set; }
}
