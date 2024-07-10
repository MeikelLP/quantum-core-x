namespace QuantumCore.API;

public interface IMall
{
    Task Load();
    void PromptPassword();
    void Open();
    void SendItems();
    void Close();
    
    DateTime? LastInteraction { get; }
}
