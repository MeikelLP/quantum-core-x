using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QuantumCore.Auth.Persistence.Entities;

[Table("account_status")]
public class AccountStatus
{
    public short Id { get; init; }
    [StringLength(8)] public required string ClientStatus { get; init; }
    [DefaultValue(false)] public required bool AllowLogin { get; init; }
    [StringLength(255)] public required string Description { get; init; }
    public ICollection<Account> Accounts { get; init; } = null!;

    public static void Configure(EntityTypeBuilder<AccountStatus> builder, DatabaseFacade database)
    {
    }
}