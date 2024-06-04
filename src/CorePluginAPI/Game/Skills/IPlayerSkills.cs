namespace QuantumCore.API.Game.Skills;

public interface IPlayerSkills
{
    Task LoadAsync();
    Task PersistAsync();
    
    ISKill? this[uint skillId] { get; }
    
    void SetSkillGroup(byte skillGroup);
    void ClearSkills();
    void ClearSubSkills();
    void Reset(uint skillId);
    void SetLevel(uint skillId, byte level);
    void SkillUp(uint skillId, ESkillLevelMethod method = ESkillLevelMethod.Point);
    bool CanUse(uint skillId);
    void SendAsync();
    bool LearnSkillByBook(uint skillId);
    void SetSkillNextReadTime(uint skillId, int time);
}
