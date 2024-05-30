using QuantumCore.API;
using QuantumCore.Game.Persistence.Entities;

namespace QuantumCore.Game.Persistence;

public interface IDbPlayerSkillsRepository : IPlayerSkillsRepository
{
    Task SavePlayerSkillAsync(PlayerSkill skill);
}
