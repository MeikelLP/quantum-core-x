using System;
using System.Linq;
using System.Threading.Tasks;
using QuantumCore.Auth.Packets;
using QuantumCore.Core;
using QuantumCore.Core.API;
using QuantumCore.Core.Constants;
using QuantumCore.Core.Logging;
using QuantumCore.Core.Networking;
using QuantumCore.Database;
using Serilog;

namespace QuantumCore.Auth
{
    internal class AuthServer : IServer
    {
        private readonly Server _server;
        private readonly DatabaseContext _dbContext;

        public AuthServer(AuthOptions options)
        {
            _dbContext = new DatabaseContext();
            _server = new Server(options.Port);
            
            PluginManager.LoadPlugins();

            // Register auth server features
            _server.RegisterNamespace("QuantumCore.Auth.Packets");
            _server.RegisterNewConnectionListener(NewConnection);

            _server.RegisterListener<LoginRequest>((connection, request) =>
            {
                var account = _dbContext.Accounts.FirstOrDefault(a => a.Username == request.Username);
                if (account == default(Account))
                {
                    Log.Debug($"Account {request.Username} not found");
                    connection.Send(new LoginFailed
                    {
                        Status = "WRONGPWD"
                    });

                    return true;
                }

                return true;
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