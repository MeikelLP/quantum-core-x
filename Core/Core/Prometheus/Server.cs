using Prometheus;
using Serilog;

namespace QuantumCore.Core.Prometheus
{
    public static class Server
    {
        private static MetricServer _server;
        
        public static void Initialize(int port)
        {
            Log.Information($"Starting prometheus metric source on :{port}");
            _server = new MetricServer(port);
            _server.Start();
        }
    }
}