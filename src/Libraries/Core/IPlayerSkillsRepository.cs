using QuantumCore.Game.Skills;

namespace QuantumCore;

public interface IPlayerSkillsRepository
{
    Task<Skill?> GetPlayerSkillAsync(uint playerId, uint skillId);
    Task<ICollection<Skill>> GetPlayerSkillsAsync(uint playerId);
}
