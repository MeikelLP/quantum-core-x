namespace QuantumCore.API.Game.Skills;

public interface IPlayerSkills
{
    Task LoadAsync();
    Task PersistAsync();

    ISKill? this[ESkillIndexes skillId] { get; }

    void SetSkillGroup(byte skillGroup);
    void ClearSkills();
    void ClearSubSkills();
    void Reset(ESkillIndexes skillId);
    void SetLevel(ESkillIndexes skillId, byte level);
    void SkillUp(ESkillIndexes skillId, ESkillLevelMethod method = ESkillLevelMethod.Point);
    bool CanUse(ESkillIndexes skillId);

    /// <summary>
    /// Send skill info to client
    /// </summary>
    void Send();

    bool LearnSkillByBook(ESkillIndexes skillId);
    void SetSkillNextReadTime(ESkillIndexes skillId, int time);
}
