namespace QuantumCore.API.Core.Models;

public class AccountData
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public short Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string DeleteCode { get; set; } = "";
    public AccountStatusData AccountStatus { get; set; } = null!;
    public DateTime? LastLogin { get; set; }
}