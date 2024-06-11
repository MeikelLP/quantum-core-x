using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Skills;

namespace QuantumCore.API;

public interface ISkillManager
{
    SkillData? GetSkill(ESkillIndexes id);
    SkillData? GetSkillByName(ReadOnlySpan<char> name);
    Task LoadAsync(CancellationToken token = default);
    Task ReloadAsync(CancellationToken token = default);
}
