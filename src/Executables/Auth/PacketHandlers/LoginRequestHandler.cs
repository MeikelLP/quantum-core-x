using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Auth.Cache;
using QuantumCore.Auth.Packets;
using QuantumCore.Caching;
using QuantumCore.Core.Utils;
using System.ComponentModel;
using QuantumCore.API.Core.Models;

namespace QuantumCore.Auth.PacketHandlers;

public enum LoginFailedBecause
{
    [Description("Account not found")] NoId,
    [Description("Wrong password")] WrongPwd,

    [Description("Account is already logged in another client")]
    Already,

    [Description("Server has reached its capacity")]
    Full,

    // Shutdown,
    // Repair,
    // Block,
    // NotAvail,
    // NoBill,
    // BlkLogin,
    // WebBlk,
    // AgeLimit,
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
                Status = LoginFailedBecause.NoId.ToString().ToUpper()
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
                status = LoginFailedBecause.WrongPwd.ToString().ToUpper();
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
            status = LoginFailedBecause.WrongPwd.ToString().ToUpper();
        }

        // Check if the account is already logged in
        var isAlreadyLoggedIn = await CheckExistingConnectionOf(account);
        if (isAlreadyLoggedIn)
        {
            status = LoginFailedBecause.Already.ToString().ToUpper();
        }


        // If the status is not empty send a failed login response to the client
        if (status != "")
        {
            ctx.Connection.Send(new LoginFailed
            {
                Status = status
            });

            return;
        }

        // Generate authentication token
        var authToken = CoreRandom.GenerateUInt32();

        // Store auth token
        await _cacheManager.Set("token:" + authToken, new Token
        {
            Username = account.Username,
            AccountId = account.Id
        });
        // Set expiration on token
        await _cacheManager.Expire("token:" + authToken, 30);

        // Relate the account ID to the token
        await _cacheManager.Set("account:token:" + account.Id, authToken);
        // Set expiration on account token
        await _cacheManager.Expire("account:token:" + account.Id, 30);

        // Send the auth token to the client and let it connect to our game server
        ctx.Connection.Send(new LoginSuccess
        {
            Key = authToken,
            Result = 1
        });
    }

    private async Task<bool> CheckExistingConnectionOf(AccountData account)
    {
        var isLoggedIn = await _cacheManager.Exists("account:token:" + account.Id);

        return isLoggedIn == 1;
    }
}
