using QuantumCore.API.Game.Skills;

namespace QuantumCore.API;

public interface IPlayerSkillsRepository
{
    Task<Skill?> GetPlayerSkillAsync(uint playerId, uint skillId);
    Task<ICollection<Skill>> GetPlayerSkillsAsync(uint playerId);
}
