using System.Collections.Immutable;
using QuantumCore.Models;

namespace QuantumCore;

public interface IMonsterManager
{
    /// <summary>
    /// Try to load mob_proto file
    /// </summary>
    Task LoadAsync(CancellationToken token = default);

    MonsterData? GetMonster(uint id);
    ImmutableArray<MonsterData> GetMonsters();
}
