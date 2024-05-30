using QuantumCore.API.Core.Models;

namespace QuantumCore.API;

public interface ISkillProtoRepository
{
    Task<SkillData?> GetSkill(uint id);
    Task<ICollection<SkillData>> GetSkills();
}
