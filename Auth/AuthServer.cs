using System;
using QuantumCore.Auth.Packets;
using QuantumCore.Core;
using QuantumCore.Core.Constants;
using QuantumCore.Core.Networking;

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
                Console.WriteLine($"Username: {request.Username}");
                Console.WriteLine($"Password: {request.Password}");
                return true;
            });
        }

        bool NewConnection(Connection connection) {
            connection.SetPhase(EPhases.Auth);
            return true;
        }

        public void Start() {
            _server.Start();

            Console.WriteLine("Press any key bla");
            Console.ReadLine();
        }
    }
}