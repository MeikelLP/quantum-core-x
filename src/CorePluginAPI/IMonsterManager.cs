using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuantumCore.API.Core.Models;

namespace QuantumCore.API;

public interface IMonsterManager
{
    /// <summary>
    /// Try to load mob_proto file
    /// </summary>
    Task LoadAsync(CancellationToken token = default);

    MonsterData GetMonster(uint id);
    List<MonsterData> GetMonsters();
}