using QuantumCore.API;
using QuantumCore.API.Game.Skills;

namespace QuantumCore.Game.Persistence;

public interface IDbPlayerSkillsRepository : IPlayerSkillsRepository
{
    Task SavePlayerSkillAsync(Skill skill);
}
