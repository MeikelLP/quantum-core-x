using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuantumCore.Auth.Persistence.Entities;

[Table("account_status")]
public class AccountStatus
{
    [Key]
    public int Id { get; set; }
    public string ClientStatus { get; set; }
    public bool AllowLogin { get; set; }
    public string Description { get; set; }
}