using System;
using QuantumCore.Auth.Packets;
using QuantumCore.Core;
using QuantumCore.Core.Constants;
using QuantumCore.Core.Logging;
using QuantumCore.Core.Networking;
using Serilog;

namespace QuantumCore.Auth
{
    class AuthServer : IServer {
        private Server _server;

        public AuthServer()
        {
            _server = new Server(11002);

            // Register auth server features
            _server.RegisterNamespace("QuantumCore.Auth.Packets");
            _server.RegisterNewConnectionListener(NewConnection);
            _server.RegisterListener<LoginRequest>((connection, request) => {
                Log.Debug($"Username: {request.Username}");
                Log.Debug($"Password: {request.Password}");
                return true;
            });
        }

        bool NewConnection(Connection connection) {
            connection.SetPhase(EPhases.Auth);
            return true;
        }

        public void Start() {
            _server.Start();

            Log.Information("Press any key bla");
            Console.ReadLine();
        }
    }
}