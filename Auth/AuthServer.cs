using System;
using System.Threading.Tasks;
using QuantumCore.Auth.Packets;
using QuantumCore.Core;
using QuantumCore.Core.Constants;
using QuantumCore.Core.Logging;
using QuantumCore.Core.Networking;
using Serilog;

namespace QuantumCore.Auth
{
    internal class AuthServer : IServer
    {
        private readonly Server _server;

        public AuthServer(AuthOptions options)
        {
            _server = new Server(options.Port);

            // Register auth server features
            _server.RegisterNamespace("QuantumCore.Auth.Packets");
            _server.RegisterNewConnectionListener(NewConnection);

            _server.RegisterListener<LoginRequest>((connection, request) =>
            {
                Log.Debug($"Username: {request.Username}");
                Log.Debug($"Password: {request.Password}");
                
                connection.Send(new LoginFailed
                {
                    Status = "WRONGPWD"
                });
                
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