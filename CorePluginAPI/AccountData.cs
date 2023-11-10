
namespace QuantumCore.API;

public class AccountData
{
    public Guid Id { get; set; }
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";

    public string Email { get; set; } = "";

    public int Status { get; set; }
        
    public DateTime? LastLogin { get; set; }

    public string DeleteCode { get; set; } = "";
    public AccountStatusData AccountStatus { get; set; }
}