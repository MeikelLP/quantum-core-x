using Core.Persistence;
using Dapper.Contrib.Extensions;

namespace QuantumCore.Game.Persistence.Entities;

[Table("shops")]
internal class Shops
{
    [ExplicitKey]
    public Guid Id { get; set; }
    [ExplicitKey]
    public uint Vnum { get; set; }
    public string Name { get; set; } = "";
}