﻿using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Services;

internal class SpawnGroupProvider : ISpawnGroupProvider
{
    public async Task<IEnumerable<SpawnGroup>> GetSpawnGroupsAsync()
    {
        const string file = "data/group.txt";
        var list = new List<SpawnGroup>();
        using var sr = new StreamReader(file);
        do
        {
            var item = await Game.ParserUtils.GetSpawnGroupFromBlock(sr);
            list.Add(item);
        } while (!sr.EndOfStream);

        return list;
    }

    public async Task<IEnumerable<SpawnGroupCollection>> GetSpawnGroupCollectionsAsync()
    {
        const string file = "data/group_group.txt";
        var list = new List<SpawnGroupCollection>();
        using var sr = new StreamReader(file);
        do
        {
            var item = await Game.ParserUtils.GetSpawnGroupCollectionFromBlock(sr);
            list.Add(item);
        } while (!sr.EndOfStream);

        return list;
    }
}
