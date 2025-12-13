using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Skills;
using QuantumCore.API.Game.Types.Skills;

namespace QuantumCore.API;

public interface ISkillManager
{
    SkillData? GetSkill(ESkill id);
    SkillData? GetSkillByName(ReadOnlySpan<char> name);
    Task ReloadAsync(CancellationToken token = default);
}
