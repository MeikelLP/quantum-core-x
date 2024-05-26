using QuantumCore.Auth.Cache;
using QuantumCore.Auth.Packets;
using QuantumCore.Caching;
using QuantumCore.Core.Utils;

namespace QuantumCore.Auth.PacketHandlers;

public class LoginRequestHandler : IAuthPacketHandler<LoginRequest>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<LoginRequestHandler> _logger;
    private readonly ICacheManager _cacheManager;

    public LoginRequestHandler(IAccountRepository accountRepository, ILogger<LoginRequestHandler> logger,
        ICacheManager cacheManager)
    {
        _accountRepository = accountRepository;
        _logger = logger;
        _cacheManager = cacheManager;
    }

    public async ValueTask ExecuteAsync(AuthPacketContext<LoginRequest> ctx, CancellationToken token = default)
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
                Status = "WRONGPWD"
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
                status = "WRONGPWD";
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
            status = "WRONGPWD";
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

        // Send the auth token to the client and let it connect to our game server
        ctx.Connection.Send(new LoginSuccess
        {
            Key = authToken,
            Result = 1
        });
    }
}