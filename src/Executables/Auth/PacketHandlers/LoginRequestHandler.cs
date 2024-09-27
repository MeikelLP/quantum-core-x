using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Auth.Cache;
using QuantumCore.Auth.Packets;
using QuantumCore.Caching;
using QuantumCore.Core.Utils;
using System.Runtime.Serialization;
using QuantumCore.API.Core.Models;
using EnumsNET;

namespace QuantumCore.Auth.PacketHandlers;

public enum LoginFailedBecause
{
    /// <summary>
    /// Invalid credentials
    /// </summary>
    [EnumMember(Value = "WRONGPWD")] InvalidCredentials,

    /// <summary>
    /// Account is already logged in
    /// </summary>
    [EnumMember(Value = "ALREADY")] AlreadyLoggedIn,

    /// <summary>
    /// Server has reached its maximum capacity
    /// TODO: implement this behavior
    /// </summary>
    [EnumMember(Value = "FULL")] Full,
}

public class LoginRequestHandler : IAuthPacketHandler<LoginRequest>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<LoginRequestHandler> _logger;
    private readonly ICacheManager _cacheManager;

    public LoginRequestHandler(IAccountRepository accountRepository, ILogger<LoginRequestHandler> logger, ICacheManager cacheManager)
    {
        _accountRepository = accountRepository;
        _logger = logger;
        _cacheManager = cacheManager;
    }

    public async Task ExecuteAsync(AuthPacketContext<LoginRequest> ctx, CancellationToken token = default)
    {
        var account = await _accountRepository.FindByNameAsync(ctx.Packet.Username);
        // Check if account was found
        if (account == default)
        {
            // Hash the password to prevent timing attacks
            BCrypt.Net.BCrypt.HashPassword(ctx.Packet.Password);

            _logger.LogDebug("Account {Username} not found", ctx.Packet.Username);
            ctx.Connection.Send(new LoginFailed
            {
                Status = LoginFailedBecause.InvalidCredentials.AsString(EnumFormat.EnumMemberValue)!
            });

            return;
        }

        var status = "";

        // Verify the password against the stored one
        try
        {
            if (!BCrypt.Net.BCrypt.Verify(ctx.Packet.Password, account.Password))
            {
                _logger.LogDebug("Wrong password supplied for account {Username}", ctx.Packet.Username);
                status = LoginFailedBecause.InvalidCredentials.AsString(EnumFormat.EnumMemberValue);
            }
            else
            {
                // Check account status stored in the database
                if (!account.AccountStatus.AllowLogin)
                {
                    status = account.AccountStatus.ClientStatus;
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning("Failed to verify password for account {Username}: {Message}", ctx.Packet.Username,
                e.Message);
            status = LoginFailedBecause.InvalidCredentials.AsString(EnumFormat.EnumMemberValue);
        }

        // Check if the account is already logged in
        var isAlreadyLoggedIn = await CheckExistingConnectionOf(account);
        if (isAlreadyLoggedIn)
        {
            status = await DecideAlreadyLoggedInStatusAsync(account);
        }


        // If the status is not empty send a failed login response to the client
        if (status != "")
        {
            ctx.Connection.Send(new LoginFailed
            {
                Status = status ?? "UNKNOWN"
            });

            return;
        }

        // Generate authentication token
        var authToken = CoreRandom.GenerateUInt32();

        // Store auth token
        await _cacheManager.Server.Set($"token:{authToken}", new Token
        {
            Username = account.Username,
            AccountId = account.Id
        });
        // Set expiration on token
        await _cacheManager.Server.Expire($"token:{authToken}", 30);

        // Relate the account ID to the token
        await _cacheManager.Shared.Set($"account:token:{account.Id}", authToken);
        // Set expiration on account token
        await _cacheManager.Shared.Expire($"account:token:{account.Id}", 30);

        // Send the auth token to the client and let it connect to our game server
        ctx.Connection.Send(new LoginSuccess
        {
            Key = authToken,
            Result = 1
        });
    }

    private async Task<bool> CheckExistingConnectionOfAsync(AccountData account)
    {
        var isLoggedIn = await _cacheManager.Shared.Exists($"account:token:{account.Id}");

        return isLoggedIn == 1;
    }
    {
        var isLoggedIn = await _cacheManager.Exists("account:token:" + account.Id);

        return isLoggedIn == 1;
    }
}
