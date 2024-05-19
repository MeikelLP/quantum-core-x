using QuantumCore.API;
using QuantumCore.API.Core.Models;

namespace QuantumCore.Auth.Persistence;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(AccountData account, string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyHash(string hash, string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}