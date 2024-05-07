using QuantumCore.API.Core.Models;
using QuantumCore.Auth.Persistence.Entities;

namespace QuantumCore.Auth.Persistence.Extensions;

public static class QueryExtensions
{
    public static IQueryable<AccountData> SelectAccountData(this IQueryable<Account> query)
    {
        return query.Select(x => new AccountData
        {
            Id = x.Id,
            Email = x.Email,
            Username = x.Username,
            Password = x.Password,
            Status = x.Status,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
            DeleteCode = x.DeleteCode,
            LastLogin = x.LastLogin,
            AccountStatus = new AccountStatusData
            {
                Id = x.AccountStatus.Id,
                Description = x.AccountStatus.Description,
                ClientStatus = x.AccountStatus.ClientStatus,
                AllowLogin = x.AccountStatus.AllowLogin
            },
        });
    }
}