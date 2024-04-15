namespace QuantumCore.API;

public class AccountStatusData
{
    public int Id { get; set; }
    public string ClientStatus { get; set; }
    public bool AllowLogin { get; set; }
    public string Description { get; set; }
}