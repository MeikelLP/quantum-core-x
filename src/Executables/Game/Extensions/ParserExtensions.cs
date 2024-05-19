using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Extensions;

internal static class ParserExtensions
{
    public static SpawnGroup ToSpawnGroup(this ParserUtils.DataFileGroup group)
    {
        return new SpawnGroup
        {
            Id = group.GetField<uint>("Vnum"),
            Leader = group.GetField<uint>("Leader"),
            Members = group.Data.Select(data => new SpawnMember
            {
                Id = uint.Parse(data.LastOrDefault()!)
            }).ToList(),
            Name = group.Name
        };
    }
    
    public static SpawnGroupCollection ToSpawnGroupCollection(this ParserUtils.DataFileGroup group)
    {
        return new SpawnGroupCollection
        {
            Id = group.GetField<uint>("Vnum"),
            Name = group.Name,
            Groups = group.Data.Select(data => new SpawnGroupCollectionMember
            {
                Id = uint.Parse(data[1]),
                Amount = byte.TryParse(data.ElementAtOrDefault(2), out var amount) ? amount : (byte)1
            }).ToList()
        };
    }
}
