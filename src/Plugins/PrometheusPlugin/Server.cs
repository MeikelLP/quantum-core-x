using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prometheus;
using QuantumCore.API.PluginTypes;

namespace PrometheusPlugin;

public class Server : ISingletonPlugin
{
    private readonly ILogger<Server> _logger;
    private readonly IConfiguration _config;
    private readonly MetricServer _server;
    private readonly int _port;

    public Server(ILogger<Server> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
        _port = _config.GetValue<int>("PrometheusPort");
        _server = new MetricServer(_port);
    }
        
    public Task InitializeAsync(CancellationToken token = default)
    {
        if (_config.GetValue<bool>("Prometheus"))
        {
            _logger.LogInformation("Starting prometheus metric source on: {Port}", _port);
            _server.Start();
        }
            
        return Task.CompletedTask;
    }
}