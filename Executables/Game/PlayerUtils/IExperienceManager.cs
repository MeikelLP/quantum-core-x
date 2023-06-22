namespace QuantumCore.Game.PlayerUtils;

public interface IExperienceManager
{
    byte MaxLevel { get; }
    uint GetNeededExperience(byte level);
    Task LoadAsync(CancellationToken token = default);
}