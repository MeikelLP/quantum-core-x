using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using QuantumCore.Auth.Packets;
using QuantumCore.Core;
using QuantumCore.Core.API;
using QuantumCore.Core.Constants;
using QuantumCore.Core.Networking;
using QuantumCore.Core.Utils;
using QuantumCore.Database;
using Serilog;

namespace QuantumCore.Auth
{
    internal class AuthServer : IServer
    {
        private readonly Server _server;

        public AuthServer(AuthOptions options)
        {
            DatabaseManager.Init(options.AccountString, options.GameString);
            _server = new Server(options.Port);
            
            PluginManager.LoadPlugins();

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
                    connection.Send(new LoginFailed
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
                }
                catch (Exception e)
                {
                    Log.Warning($"Failed to verify password for account {request.Username}: {e.Message}");
                    status = "WRONGPWD";
                }

                // If the status is not empty send a failed login response to the client
                if (status != "")
                {
                    connection.Send(new LoginFailed
                    {
                        Status = status
                    });

                    return;
                }
                
                // Generate authentication token
                var authToken = CoreRandom.GenerateUInt32();
                
                // Send the auth token to the client and let it connect to our game server
                connection.Send(new LoginSuccess
                {
                    Key = authToken,
                    Result = 1
                });
            });
        }

        public async Task Start()
        {
            await _server.Start();
        }

        private bool NewConnection(Connection connection)
        {
            connection.SetPhase(EPhases.Auth);
            return true;
        }
    }
}