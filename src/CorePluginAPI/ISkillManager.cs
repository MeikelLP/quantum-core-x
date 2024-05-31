using QuantumCore.API.Core.Models;

namespace QuantumCore.API;

public interface ISkillManager
{
    SkillData? GetSkill(uint id);
    SkillData? GetSkillByName(ReadOnlySpan<char> name);
    Task LoadAsync(CancellationToken token = default);
    Task ReloadAsync(CancellationToken token = default);
}
