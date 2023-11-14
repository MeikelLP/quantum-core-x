namespace QuantumCore.API;

public interface IPasswordHasher
{
    string HashPassword(AccountData account, string password);
    bool VerifyHash(string hash, string password);
}