namespace QuantumCore.API;

public interface IGuildExperienceManager
{
    byte MaxLevel { get; }
    uint GetNeededExperience(byte level);
    ushort GetMaxPlayers(byte level);
    Task LoadAsync(CancellationToken token = default);
}