using QuantumCore.Game.Types.Skills;
using QuantumCore.Models;

namespace QuantumCore;

public interface ISkillManager
{
    SkillData? GetSkill(ESkill id);
    SkillData? GetSkillByName(ReadOnlySpan<char> name);
    Task ReloadAsync(CancellationToken token = default);
}
