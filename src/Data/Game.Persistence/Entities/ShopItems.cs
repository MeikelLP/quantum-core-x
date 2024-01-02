using Core.Persistence;
using Dapper.Contrib.Extensions;

namespace QuantumCore.Game.Persistence.Entities;

[Table("shop_items")]
public class ShopItems
{
    public Guid ShopId { get; set; }
    public uint ItemId { get; set; }
    public uint Count { get; set; }
}