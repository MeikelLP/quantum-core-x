using QuantumCore.API.Game.World;
using QuantumCore.Game.Services;

namespace QuantumCore.Game.Extensions;

internal static class ParserExtensions
{
    public static SpawnGroup ToSpawnGroup(this ParserService.DataFileGroup group)
    {
        return new SpawnGroup
        {
            Id = group.GetField<uint>("Vnum"),
            Leader = group.GetField<uint>("Leader"),
            Members = group.Data.Select(data => new SpawnMember {Id = uint.Parse(data.LastOrDefault()!)}).ToList(),
            Name = group.Name
        };
    }

    public static SpawnGroupCollection ToSpawnGroupCollection(this ParserService.DataFileGroup group)
    {
        return new SpawnGroupCollection
        {
            Id = group.GetField<uint>("Vnum"),
            Name = group.Name,
            Groups = group.Data.Select(data =>
            {
                var p = byte.TryParse(data.ElementAtOrDefault(2), out var probability) && probability > 1
                    ? (float)(probability / 100m)
                    : 1;
                return new SpawnGroupCollectionMember {Id = uint.Parse(data[1]), Probability = p};
            }).ToList()
        };
    }
}
