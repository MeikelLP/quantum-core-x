using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game;

namespace ExamplePlugin
{
    public class Plugin : ISingletonPlugin
    {
        private readonly ILogger<Plugin> _logger;
        private readonly IGame _server;
        public string Name { get; } = "ExamplePlugin";
        public string Author { get; } = "QuantumCore Contributors";

        public Plugin(ILogger<Plugin> logger, IGame server)
        {
            _logger = logger;
            _server = server;
        }

        public Task InitializeAsync(CancellationToken token = default)
        {
            _logger.LogInformation("ExamplePlugin register!");

            _server.RegisterCommandNamespace(typeof(TestCommand));
            
            return Task.CompletedTask;
        }
    }
}