using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuantumCore.API.Game.Types;
using QuantumCore.Auth.Cache;
using QuantumCore.Auth.Packets;
using QuantumCore.Cache;
using QuantumCore.Core.API;
using QuantumCore.Core.Networking;
using QuantumCore.Core.Utils;
using QuantumCore.Database;

namespace QuantumCore.Auth
{
    public class AuthServer : ServerBase<AuthConnection>
    {
        private readonly ILogger<AuthServer> _logger;
        private readonly AuthOptions _options;

        public AuthServer(IServiceProvider serviceProvider, IOptions<AuthOptions> options, IPacketManager packetManager, ILogger<AuthServer> logger) 
            : base(serviceProvider, packetManager, logger, options.Value.Port)
        {
            _logger = logger;
            _options = options.Value;
        }

        protected async override Task ExecuteAsync(CancellationToken token)
        {
            // Initialize static components
            DatabaseManager.Init(_options.AccountString, _options.GameString);
            CacheManager.Init(_options.RedisHost, _options.RedisPort);
            
            // Load and init all plugins
            PluginManager.LoadPlugins(this);

            // Register auth server features
            PacketManager.RegisterNamespace("QuantumCore.Auth.Packets");
            RegisterNewConnectionListener(NewConnection);

            RegisterListener<LoginRequest>(async (connection, request) =>
            {
                using var db = DatabaseManager.GetAccountDatabase();
                var account = await db.QueryFirstOrDefaultAsync<Account>(
                    "SELECT * FROM accounts WHERE Username = @Username", new {Username = request.Username});
                // Check if account was found
                if (account == default(Account))
                {
                    // Hash the password to prevent timing attacks
                    BCrypt.Net.BCrypt.HashPassword(request.Password);
                    
                    _logger.LogDebug($"Account {request.Username} not found");
                    await connection.Send(new LoginFailed
                    {
                        Status = "WRONGPWD"
                    });

                    return;
                }
                
                var status = "";

                // Verify the password against the stored one
                try
                {
                    if (!BCrypt.Net.BCrypt.Verify(request.Password, account.Password))
                    {
                        _logger.LogDebug($"Wrong password supplied for account {request.Username}");
                        status = "WRONGPWD";
                    }
                    else
                    {
                        // Check account status stored in the database
                        var dbStatus = await db.GetAsync<AccountStatus>(account.Status);
                        if (!dbStatus.AllowLogin)
                        {
                            status = dbStatus.ClientStatus;
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogWarning($"Failed to verify password for account {request.Username}: {e.Message}");
                    status = "WRONGPWD";
                }

                // If the status is not empty send a failed login response to the client
                if (status != "")
                {
                    await connection.Send(new LoginFailed
                    {
                        Status = status
                    });

                    return;
                }
                
                // Generate authentication token
                var authToken = CoreRandom.GenerateUInt32();
                
                // Store auth token
                await CacheManager.Instance.Set("token:" + authToken, new Token
                {
                    Username = account.Username,
                    AccountId = account.Id
                });
                // Set expiration on token
                await CacheManager.Instance.Expire("token:" + authToken, 30);
                
                // Send the auth token to the client and let it connect to our game server
                await connection.Send(new LoginSuccess
                {
                    Key = authToken,
                    Result = 1
                });
            });
            
            var pong = await CacheManager.Instance.Ping();
            if (!pong)
            {
                _logger.LogError("Failed to ping redis server");
            }
        }

        private async Task<bool> NewConnection(Connection connection)
        {
            await connection.SetPhase(EPhases.Auth);
            return true;
        }
    }
}