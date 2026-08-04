using Microsoft.Extensions.Logging;
using QuantumCore.API;

namespace QuantumCore.Game;

public class SessionManager : ILoadable
{
    private readonly ILogger _logger;
    private readonly ICacheManager _cacheManager;
    private readonly IGameServer _server;

    public SessionManager(ILogger<SessionManager> logger, ICacheManager cacheManager, IGameServer server)
    {
        _logger = logger;
        _cacheManager = cacheManager;
        _server = server;
    }

    private void OnAuthDrop(Guid accountId)
    {
        var connection = _server.Connections.FirstOrDefault(x => x.AccountId == accountId);

        connection?.Close();
    }

    public async Task LoadAsync(CancellationToken token = default)
    {
        _logger.LogInformation("Initialize session manager");
        var sharedSubscriber = _cacheManager.Shared.Subscribe();
        var serverSubscriber = _cacheManager.Server.Subscribe();

        // Drop all pre-existing tokens
        await _cacheManager.Shared.DelAllAsync("account:*");
        await _cacheManager.Server.DelAllAsync("account:*");
        await _cacheManager.Server.DelAllAsync("token:*");

        // Register the session message handlers
        sharedSubscriber.Register<Guid>("account:drop-connection", OnAuthDrop);

        // Listen for session messages
        sharedSubscriber.Listen();
        serverSubscriber.Listen();
    }
}