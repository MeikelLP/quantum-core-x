namespace QuantumCore;

public interface IExperienceManager
{
    byte MaxLevel { get; }
    uint GetNeededExperience(byte level);
}
