using QuantumCore.API.Game.Skills;

namespace QuantumCore.API;

public interface IPlayerSkillsRepository
{
    Task<IPlayerSkill?> GetPlayerSkillAsync(uint playerId, uint skillId);
    Task<ICollection<IPlayerSkill>> GetPlayerSkillsAsync(uint playerId);
}
