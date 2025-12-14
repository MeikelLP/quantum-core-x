namespace QuantumCore.Game.Commands;

public class GameCommandOptions
{
    public const string CONFIG_SECTION = "Game:Commands";

    /// <summary>
    /// Will crash if a command cannot be found. Useful for testing but not in production
    /// </summary>
    public bool StrictMode { get; set; }
}