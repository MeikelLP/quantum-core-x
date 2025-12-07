using QuantumCore.API.Game.Types.Skills;

namespace QuantumCore.API.Game.Skills;

public interface IPlayerSkills
{
    Task LoadAsync();
    Task PersistAsync();

    ISkill? this[ESkill skillId] { get; }

    void SetSkillGroup(byte skillGroup);
    void ClearSkills();
    void ClearSubSkills();
    void Reset(ESkill skillId);
    void SetLevel(ESkill skillId, byte level);
    void SkillUp(ESkill skillId, ESkillLevelMethod method = ESkillLevelMethod.Point);
    bool CanUse(ESkill skillId);

    /// <summary>
    /// Send skill info to client
    /// </summary>
    void Send();

    bool LearnSkillByBook(ESkill skillId);
    void SetSkillNextReadTime(ESkill skillId, int time);
}
