using System;
using QuantumCore.Auth.Packets;
using QuantumCore.Core;
using QuantumCore.Core.Constants;
using QuantumCore.Core.Networking;

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
                Console.WriteLine($"Username: {request.Username}");
                Console.WriteLine($"Password: {request.Password}");
                return true;
            });
        }

        public void Start()
        {
            _server.Start();

            Console.WriteLine("Press any key bla");
            Console.ReadLine();
        }

        private bool NewConnection(Connection connection)
        {
            connection.SetPhase(EPhases.Auth);
            return true;
        }
    }
}