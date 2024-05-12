using Microsoft.EntityFrameworkCore;
using QuantumCore.API.Core.Models;
using QuantumCore.Auth.Persistence.Entities;
using QuantumCore.Auth.Persistence.Extensions;

namespace QuantumCore.Auth.Persistence;

public class AccountRepository : IAccountRepository
{
    private readonly AuthDbContext _db;

    public AccountRepository(AuthDbContext db)
    {
        _db = db;
    }

    public async Task<AccountData?> FindByNameAsync(string userName)
    {
        return await _db.Accounts
            .AsNoTracking()
            .Where(x => x.Username == userName)
            .SelectAccountData()
            .FirstOrDefaultAsync();
    }

    public async Task<AccountData?> FindByIdAsync(Guid id)
    {
        return await _db.Accounts
            .AsNoTracking()
            .Where(x => x.Id == id)
            .SelectAccountData()
            .FirstOrDefaultAsync();
    }

    public async Task<AccountData> CreateAsync(AccountData account)
    {
        var entity = new Account
        {
            Email = account.Email,
            Username = account.Username,
            Password = account.Password,
            Status = account.Status,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt,
            DeleteCode = account.DeleteCode,
        };
        _db.Accounts.Add(entity);
        await _db.SaveChangesAsync();
        await _db.Entry(entity).Reference(x => x.AccountStatus).LoadAsync();

        return new AccountData
        {
            Id = entity.Id,
            Email = entity.Email,
            Username = entity.Username,
            Password = entity.Password,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            DeleteCode = entity.DeleteCode,
            AccountStatus = new AccountStatusData
            {
                Id = entity.AccountStatus.Id,
                Description = entity.AccountStatus.Description,
                AllowLogin = entity.AccountStatus.AllowLogin,
                ClientStatus = entity.AccountStatus.ClientStatus,
            }
        };
    }
}
