using QuantumCore.API;
using QuantumCore.API.Game.Skills;
using QuantumCore.Game.Persistence.Entities;

namespace QuantumCore.Game.Persistence;

public interface IDbPlayerSkillsRepository : IPlayerSkillsRepository
{
    Task SavePlayerSkillAsync(Skill skill);
}
