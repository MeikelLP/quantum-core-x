using System;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.WebApi;
using QuantumCore.API;
using QuantumCore.API.Game;
using Serilog;
using Swan.Logging;
using ILogger = Serilog.ILogger;

namespace RestServicePlugin
{
    public class Plugin : IPlugin
    {
        public string Name { get; } = "RestServicePlugin";
        public string Author { get; } = "QuantumCore Contributors";

        private WebServer _server;
        private Task _serverTask;
        private IGame _game;
        
        public void Register(object server)
        {
            if (server is IGame game)
            {
                _game = game;
            }
            else
            {
                Log.Error("RestServicePlugin not loaded because it's not supported by the core type");
                return;
            }
            
            Logger.UnregisterLogger<ConsoleLogger>();

            Log.Debug("Starting rest service on localhost:8080!");
            _server = CreateWebServer();
            _serverTask = _server.RunAsync();
        }

        public void Unregister()
        {
            Log.Debug("Stopping server");
            _server?.Dispose();
        }

        private WebServer CreateWebServer()
        {
            var server = new WebServer(o => o.WithUrlPrefix("http://localhost:8080").WithMode(HttpListenerMode.EmbedIO))
                .WithLocalSessionManager()
                .WithCors()
                .WithWebApi("/api", m => m.WithController<ApiController>(() => new ApiController(_game)))
                .WithModule(new LiveWebSocket("/ws", _game));
            server.StateChanged += (s, e) => Log.Debug($"WebServer New State - {e.NewState}");
            return server;
        }
    }
}