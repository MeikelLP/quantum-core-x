using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Auth.Cache;
using QuantumCore.Auth.Packets;
using QuantumCore.Core.Cache;
using QuantumCore.Core.Utils;
using QuantumCore.Database;

namespace QuantumCore.Auth.PacketHandlers;

public class LoginRequestHandler : IAuthPacketHandler<LoginRequest>
{
    private readonly IDbConnection _db;
    private readonly ILogger<LoginRequestHandler> _logger;
    private readonly ICacheManager _cacheManager;

    public LoginRequestHandler(IDbConnection db, ILogger<LoginRequestHandler> logger, ICacheManager cacheManager)
    {
        _db = db;
        _logger = logger;
        _cacheManager = cacheManager;
    }

    public async Task ExecuteAsync(AuthPacketContext<LoginRequest> ctx, CancellationToken token = default)
    {
        var account = await _db.QueryFirstOrDefaultAsync<Account>(
            "SELECT * FROM account.accounts WHERE Username = @Username", new {Username = ctx.Packet.Username});
        // Check if account was found
        if (account == default(Account))
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
                var dbStatus = await _db.GetAsync<AccountStatus>(account.Status);
                if (!dbStatus.AllowLogin)
                {
                    status = dbStatus.ClientStatus;
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning("Failed to verify password for account {Username}: {Message}", ctx.Packet.Username, e.Message);
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
