using System.Collections.Immutable;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.Game.Drops;
using QuantumCore.Game.World;

namespace QuantumCore.Game.Services;

public interface IParserService
{
    SpawnPoint? GetSpawnFromLine(string line);
    Task<ImmutableArray<CommonDropEntry>> GetCommonDropsAsync(TextReader sr, CancellationToken cancellationToken = default);
    Task<ImmutableArray<SkillData>> GetSkillsAsync(string path, CancellationToken token = default);
    Task<List<ParserService.DataFileGroup>> ParseFileGroups(StreamReader sr);
    MonsterDropContainer? ParseMobGroup(ParserService.DataFileGroup group, IItemManager itemManager);
}
