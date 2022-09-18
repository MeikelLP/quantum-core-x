using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using QuantumCore.API.Game.Types;
using QuantumCore.Auth.Cache;
using QuantumCore.Auth.Packets;
using QuantumCore.Cache;
using QuantumCore.Core;
using QuantumCore.Core.API;
using QuantumCore.Core.Constants;
using QuantumCore.Core.Networking;
using QuantumCore.Core.Utils;
using QuantumCore.Database;
using Serilog;

namespace QuantumCore.Auth
{
    internal class AuthServer : IHostedService
    {
        private readonly AuthOptions _options;
        private Server<AuthConnection> _server;

        public AuthServer(IOptions<AuthOptions> options)
        {
            _options = options.Value;
        }

        public async Task StartAsync(CancellationToken token)
        {

            // Initialize static components
            DatabaseManager.Init(_options.AccountString, _options.GameString);
            CacheManager.Init(_options.RedisHost, _options.RedisPort);
            
            // Start tcp server
            _server = new Server<AuthConnection>((server, client) => new AuthConnection(server, client), _options.Port);
            
            // Load and init all plugins
            PluginManager.LoadPlugins(this);

            // Register auth server features
            _server.RegisterNamespace("QuantumCore.Auth.Packets");
            _server.RegisterNewConnectionListener(NewConnection);

            _server.RegisterListener<LoginRequest>(async (connection, request) =>
            {
                using var db = DatabaseManager.GetAccountDatabase();
                var account = await db.QueryFirstOrDefaultAsync<Account>(
                    "SELECT * FROM accounts WHERE Username = @Username", new {Username = request.Username});
                // Check if account was found
                if (account == default(Account))
                {
                    // Hash the password to prevent timing attacks
                    BCrypt.Net.BCrypt.HashPassword(request.Password);
                    
                    Log.Debug($"Account {request.Username} not found");
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
                        Log.Debug($"Wrong password supplied for account {request.Username}");
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
                    Log.Warning($"Failed to verify password for account {request.Username}: {e.Message}");
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
                Log.Error("Failed to ping redis server");
            }
            
            await _server.Start();
        }

        private async Task<bool> NewConnection(Connection connection)
        {
            await connection.SetPhase(EPhases.Auth);
            return true;
        }

        public Task StopAsync(CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}