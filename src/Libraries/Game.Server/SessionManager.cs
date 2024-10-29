using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.Caching;

namespace QuantumCore.Game;

public class SessionManager : ISessionManager
{
    private readonly ILogger _logger;
    private readonly ICacheManager _cacheManager;
    private IGameServer? _gameServer;

    public SessionManager(ILogger<SessionManager> logger, ICacheManager cacheManager)
    {
        _logger = logger;
        _cacheManager = cacheManager;
    }

    public void Init(IGameServer gameServer)
    {
        _logger.LogInformation("Initialize session manager");
        _gameServer = gameServer;
        var sharedSubscriber = _cacheManager.Shared.Subscribe();
        var serverSubscriber = _cacheManager.Server.Subscribe();

        // Drop all pre-existing tokens
        _cacheManager.Shared.DelAllAsync("account:*");
        _cacheManager.Server.DelAllAsync("account:*");
        _cacheManager.Server.DelAllAsync("token:*");

        // Register the session message handlers
        sharedSubscriber.Register<Guid>("account:drop-connection", OnAuthDropAsync);

        // Listen for session messages
        sharedSubscriber.Listen();
        serverSubscriber.Listen();
    }

    private void OnAuthDropAsync(Guid accountId)
    {
        var connection = _gameServer?.Connections.FirstOrDefault(x => x.AccountId == accountId);

        connection?.Close();
    }
}