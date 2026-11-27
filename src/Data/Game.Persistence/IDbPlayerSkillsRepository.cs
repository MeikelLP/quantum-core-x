using QuantumCore.Game.Skills;

namespace QuantumCore.Game.Persistence;

public interface IDbPlayerSkillsRepository : IPlayerSkillsRepository
{
    Task SavePlayerSkillAsync(Skill skill);
}
