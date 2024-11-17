using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Skills;

namespace QuantumCore.API;

public interface ISkillManager
{
    SkillData? GetSkill(ESkillIndexes id);
    SkillData? GetSkillByName(ReadOnlySpan<char> name);
    Task ReloadAsync(CancellationToken token = default);
}
