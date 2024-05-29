namespace QuantumCore.API.Game.Skills;

public interface IPlayerSkills
{
    Task LoadAsync();
    Task SaveAsync();
    
    IPlayerSkill? this[uint skillId] { get; set; }
    
    void SetSkillGroup(byte skillGroup);
    void ClearSkills();
    void ClearSubSkills();
    void Reset(uint skillId);
    void SetLevel(uint skillId, byte level);
    void SkillUp(uint skillId);
    bool CanUse(uint skillId);
}
