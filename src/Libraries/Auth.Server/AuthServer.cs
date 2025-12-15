using QuantumCore.API;
using QuantumCore.API.Game.Types;
using QuantumCore.Caching;
using QuantumCore.Core.Networking;
using QuantumCore.Extensions;
using QuantumCore.Networking;

namespace QuantumCore.Auth;

public class AuthServer : ServerBase<AuthConnection>
{
    private readonly ILogger<AuthServer> _logger;
    private readonly ICacheManager _cacheManager;

    public AuthServer([FromKeyedServices(HostingOptions.MODE_AUTH)] IPacketManager packetManager,
        ILogger<AuthServer> logger,
        PluginExecutor pluginExecutor, IServiceProvider serviceProvider, ICacheManager cacheManager)
        : base(packetManager, logger, pluginExecutor, serviceProvider, HostingOptions.MODE_AUTH)
    {
        _logger = logger;
        _cacheManager = cacheManager;
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        // Register auth server features
        RegisterNewConnectionListener(NewConnection);

        var pong = await _cacheManager.Ping();
        if (!pong)
        {
            _logger.LogError("Failed to ping redis server");
        }
    }

    private bool NewConnection(IConnection connection)
    {
        connection.SetPhase(EPhase.AUTH);
        return true;
    }
}
