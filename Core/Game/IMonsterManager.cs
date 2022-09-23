using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuantumCore.Core.Types;

namespace QuantumCore.Game;

public interface IMonsterManager
{
    /// <summary>
    /// Try to load mob_proto file
    /// </summary>
    Task LoadAsync(CancellationToken token = default);

    MobProto.Monster GetMonster(uint id);
    List<MobProto.Monster> GetMonsters();
}